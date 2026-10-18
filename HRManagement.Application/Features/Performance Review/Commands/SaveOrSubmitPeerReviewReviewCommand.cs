using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Commands
{
    public record SaveOrSubmitPeerReviewReviewCommand(
        SaveOrSubmitPeerReviewReviewPayload Request,
        int CurrentUserId
    ) : IRequest<Result>;

    internal sealed class SaveOrSubmitPeerReviewReviewCommandHandler(
        IAssignmentRepository assignmentRepository,
        ILogger<SaveOrSubmitPeerReviewReviewCommandHandler> logger,
        IUnitOfWork unitOfWork) : IRequestHandler<SaveOrSubmitPeerReviewReviewCommand, Result>
    {
        public async Task<Result> Handle(SaveOrSubmitPeerReviewReviewCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Executing handler: {HandlerName}", nameof(SaveOrSubmitPeerReviewReviewCommandHandler));

            var req = command.Request;
            var currentUserId = command.CurrentUserId;

            // 1. Fetch all peer-review fill assignments for this subject and interval
            var peerFillAssignments = await assignmentRepository.FindPeerReviewFillAssignmentsAsync(
                req.IntervalId,
                req.SubjectId,
                cancellationToken);

            if (!peerFillAssignments.Any())
            {
                return Result.Failure("Penugasan peer review untuk karyawan ini tidak ditemukan.");
            }

            var fillAssignmentIds = peerFillAssignments.Select(fa => fa.Id).ToList();

            // 2. Fetch all peer answers tied to these fill assignments to map AnswerId -> FillAssignmentId
            var allAnswers = await assignmentRepository.GetAnswersByAssignmentIdsAsync(fillAssignmentIds, cancellationToken);
            var answerToAssignmentMap = allAnswers.ToDictionary(a => a.Id, a => a.AssignmentId);

            // Validate that all incoming scores belong to valid peer answers
            foreach (var reviewScore in req.ReviewScores)
            {
                if (!answerToAssignmentMap.ContainsKey(reviewScore.AssessmentAnswerId))
                {
                    return Result.Failure($"Jawaban dengan ID {reviewScore.AssessmentAnswerId} tidak valid.");
                }

                if (!req.IsDraft)
                {
                    if (reviewScore.Score < 1 || reviewScore.Score > 5)
                    {
                        return Result.Failure("Rating supervisor wajib diisi antara 1 sampai 5.");
                    }
                }
            }

            // 3. (Optional to keep for score mapping) Group incoming scores by their target FillAssignment ID
            var scoresByAssignment = req.ReviewScores
                .GroupBy(score => answerToAssignmentMap[score.AssessmentAnswerId])
                .ToDictionary(g => g.Key, g => g.ToList());

            // Fetch existing ReviewAssignments for this supervisor across these fill assignments
            var existingReviewAssignments = await assignmentRepository.FindReviewAssignmentsByAssignmentIdsAsync(
                fillAssignmentIds,
                currentUserId,
                cancellationToken);

            var reviewAssignmentMap = existingReviewAssignments.ToDictionary(ra => ra.AssignmentId);

            // Fetch reviewer metadata once for creating review assignments
            var (reviewerRoleId, reviewerJobTitle) = await assignmentRepository.GetReviewerMetadataAsync(currentUserId, cancellationToken);
            string initialStatus = req.IsDraft ? AssignmentStatusConstant.InProgress : AssignmentStatusConstant.Reviewed;

            // 4. Process ALL peer assignments (Locks/claims all of them for this supervisor at once)
            foreach (var fillAssignment in peerFillAssignments)
            {
                int fillAssignmentId = fillAssignment.Id;

                if (!reviewAssignmentMap.TryGetValue(fillAssignmentId, out var reviewAssignment))
                {
                    // Create and lock/claim the ReviewAssignment for this supervisor for EVERY peer
                    reviewAssignment = new ReviewAssignment(
                        planId: fillAssignment.PlanId,
                        intervalId: req.IntervalId,
                        assignmentId: fillAssignmentId,
                        reviewerRoleId: reviewerRoleId ?? 1,
                        reviewerJobTitle: reviewerJobTitle,
                        reviewerId: currentUserId,
                        initialStatus: initialStatus,
                        actionerId: currentUserId
                    );

                    await assignmentRepository.AddReviewAssignmentAsync(reviewAssignment, cancellationToken);
                    await unitOfWork.CommitAsync(cancellationToken); // Commit to generate ID for ReviewAssignment
                }

                // Check if this specific peer has incoming scores in the request payload
                if (scoresByAssignment.TryGetValue(fillAssignmentId, out var assignmentScores))
                {
                    var scoresToPersist = assignmentScores.Select(s => new AssessmentAnswerScore(
                        reviewAssignmentId: reviewAssignment.Id,
                        assessmentAnswerId: s.AssessmentAnswerId,
                        score: s.Score,
                        reviewerId: currentUserId,
                        actionerId: currentUserId
                    )).ToList();

                    // Upsert scores for this peer
                    await assignmentRepository.UpsertAnswerScoresAsync(reviewAssignment.Id, scoresToPersist, cancellationToken);
                }

                // Update supervisor review status
                reviewAssignment.UpdateStatus(initialStatus, currentUserId);

                // --- ADD THIS BLOCK IF THE PEER'S FILL ASSIGNMENT SHOULD ALSO UPDATE ---
                if (!req.IsDraft)
                {
                    // If the supervisor fully submits, mark the peer's fill assignment as Completed/Reviewed
                    fillAssignment.ApplyUpdate(AssignmentStatusConstant.Reviewed, currentUserId);
                }
            }

            await unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}