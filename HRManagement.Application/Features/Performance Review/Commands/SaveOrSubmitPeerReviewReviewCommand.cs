using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
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

            // 3. Group incoming scores by their target FillAssignment ID
            var scoresByAssignment = req.ReviewScores
                .GroupBy(score => answerToAssignmentMap[score.AssessmentAnswerId]);

            // Fetch existing ReviewAssignments for this supervisor across these fill assignments
            var existingReviewAssignments = await assignmentRepository.FindReviewAssignmentsByAssignmentIdsAsync(
                fillAssignmentIds,
                currentUserId,
                cancellationToken);

            var reviewAssignmentMap = existingReviewAssignments.ToDictionary(ra => ra.AssignmentId);

            // Fetch reviewer metadata once in case we need to create new review assignments
            var (reviewerRoleId, reviewerJobTitle) = await assignmentRepository.GetReviewerMetadataAsync(currentUserId, cancellationToken);
            string initialStatus = req.IsDraft ? "drafted" : "scored";

            // 4. Process each peer assignment's review scores and status
            foreach (var group in scoresByAssignment)
            {
                int fillAssignmentId = group.Key;
                var fillAssignment = peerFillAssignments.First(fa => fa.Id == fillAssignmentId);

                if (!reviewAssignmentMap.TryGetValue(fillAssignmentId, out var reviewAssignment))
                {
                    // Create and lock/claim the ReviewAssignment for this supervisor
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
                else
                {
                    string currentStatus = reviewAssignment.Status?.ToLower() ?? "not started";
                    if (currentStatus == "scored" && !req.IsDraft)
                    {
                        // Optional: prevent overwriting final submission unless intended
                    }
                }

                // Build scores list for this specific review assignment
                var scoresToPersist = group.Select(s => new AssessmentAnswerScore(
                    reviewAssignmentId: reviewAssignment.Id,
                    assessmentAnswerId: s.AssessmentAnswerId,
                    score: s.Score,
                    reviewerId: currentUserId,
                    actionerId: currentUserId
                )).ToList();

                // Upsert scores
                await assignmentRepository.UpsertAnswerScoresAsync(reviewAssignment.Id, scoresToPersist, cancellationToken);

                // Update status (locks/claims or marks as drafted/scored)
                reviewAssignment.UpdateStatus(initialStatus, currentUserId);
            }

            await unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}