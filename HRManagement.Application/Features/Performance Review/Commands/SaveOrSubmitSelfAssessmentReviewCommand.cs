using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Commands
{
    public record SaveOrSubmitSelfAssessmentReviewCommand(
        SaveOrSubmitSelfAssessmentReviewPayload Request,
        int CurrentUserId
    ) : IRequest<Result<int>>;

    internal sealed class SaveOrSubmitSelfAssessmentReviewCommandHandler(
        IAssignmentRepository assignmentRepository,
        ILogger<SaveOrSubmitSelfAssessmentReviewCommandHandler> logger,
        IUnitOfWork unitOfWork) : IRequestHandler<SaveOrSubmitSelfAssessmentReviewCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(SaveOrSubmitSelfAssessmentReviewCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Executing handler: {HandlerName}", nameof(SaveOrSubmitSelfAssessmentReviewCommandHandler));

            var req = command.Request;
            var currentUserId = command.CurrentUserId;

            // 1. Find the Employee's Self-Assessment FillAssignment record
            var selfFillAssignment = await assignmentRepository.FindSelfAssessmentFillAssignmentAsync(
                req.IntervalId,
                req.SubjectId,
                cancellationToken);

            if (selfFillAssignment == null)
            {
                return Result.Failure<int>("Penugasan self assessment untuk karyawan ini tidak ditemukan.");
            }

            // 2. Find or Create the Supervisor's ReviewAssignment record linked to this FillAssignment
            var reviewAssignment = await assignmentRepository.FindReviewAssignmentAsync(
                selfFillAssignment.PlanId,
                req.IntervalId,
                selfFillAssignment.Id,
                currentUserId,
                cancellationToken);

            int reviewAssignmentId;

            if (reviewAssignment == null)
            {
                string initialStatus = req.IsDraft ? "drafted" : "in progress";

                // Fetch metadata directly using your new repository method
                var (reviewerRoleId, reviewerJobTitle) = await assignmentRepository.GetReviewerMetadataAsync(currentUserId, cancellationToken);

                reviewAssignment = new ReviewAssignment(
                    planId: selfFillAssignment.PlanId,
                    intervalId: req.IntervalId,
                    assignmentId: selfFillAssignment.Id,
                    reviewerRoleId: reviewerRoleId ?? 1,
                    reviewerJobTitle: reviewerJobTitle,
                    reviewerId: currentUserId,
                    initialStatus: initialStatus,
                    actionerId: currentUserId
                );

                await assignmentRepository.AddReviewAssignmentAsync(reviewAssignment, cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                reviewAssignmentId = reviewAssignment.Id;
            }
            else
            {
                reviewAssignmentId = reviewAssignment.Id;

                string currentStatus = reviewAssignment.Status?.ToLower() ?? "not started";
                if (currentStatus == "done" || currentStatus == "submitted")
                {
                    return Result.Failure<int>("Review self-assessment ini sudah dikirimkan sebelumnya dan tidak dapat diubah kembali.");
                }
            }

            // 3. Map QuestionIds to existing AssessmentAnswerIds from the employee's self-assessment
            var employeeAnswers = await assignmentRepository.GetAnswersByAssignmentIdAsync(selfFillAssignment.Id, cancellationToken);
            var questionToAnswerIdMap = employeeAnswers.ToDictionary(a => a.AssessmentQuestionId, a => a.Id);

            var scoresToPersist = new List<AssessmentAnswerScore>();

            foreach (var reviewScore in req.ReviewScores)
            {
                if (!questionToAnswerIdMap.TryGetValue(reviewScore.QuestionId, out int assessmentAnswerId))
                {
                    return Result.Failure<int>($"Jawaban untuk pertanyaan ID {reviewScore.QuestionId} tidak ditemukan pada self assessment karyawan.");
                }

                if (!req.IsDraft)
                {
                    if (!reviewScore.SupervisorRating.HasValue || reviewScore.SupervisorRating.Value < 1 || reviewScore.SupervisorRating.Value > 5)
                    {
                        return Result.Failure<int>($"Rating supervisor untuk pertanyaan ID {reviewScore.QuestionId} wajib diisi antara 1 sampai 5.");
                    }
                }

                scoresToPersist.Add(new AssessmentAnswerScore(
                    reviewAssignmentId: reviewAssignmentId,
                    assessmentAnswerId: assessmentAnswerId,
                    score: reviewScore.SupervisorRating ?? 0,
                    reviewerId: currentUserId,
                    actionerId: currentUserId
                ));
            }

            // 4. Upsert Scores and Finalize Status
            await assignmentRepository.UpsertAnswerScoresAsync(reviewAssignmentId, scoresToPersist, cancellationToken);

            string finalStatus = req.IsDraft ? "drafted" : "scored";
            reviewAssignment.UpdateStatus(finalStatus, currentUserId);

            await unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(reviewAssignmentId);
        }
    }
}