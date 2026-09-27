using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Commands;

public record SubmitTextReviewCommand(
    SubmitTextReviewPayload Request,
    int CurrentUserId
) : IRequest<Result<int>>;

internal sealed class SubmitTextReviewCommandHandler(
    IAssignmentRepository assignmentRepository,
    ILogger<SubmitTextReviewCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<SubmitTextReviewCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SubmitTextReviewCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler: {HandlerName}", nameof(SubmitTextReviewCommandHandler));

        var req = command.Request;
        var reviewerId = command.CurrentUserId;

        if (req.ReviewScores == null || !req.ReviewScores.Any())
        {
            return Result.Failure<int>("Tidak ada nilai jawaban yang dikirimkan.");
        }

        var incomingAnswerIds = req.ReviewScores.Select(r => r.AssessmentAnswerId).Distinct().ToList();

        // 1. Fetch all assessment answers and their parent FillAssignments
        var answers = await assignmentRepository.GetAnswersByIdsAsync(incomingAnswerIds, cancellationToken);
        if (answers.Count != incomingAnswerIds.Count)
        {
            return Result.Failure<int>("Satu atau lebih ID jawaban tidak ditemukan dalam sistem.");
        }

        // Group answers by FillAssignmentId
        var answersByAssignment = answers.GroupBy(a => a.AssignmentId).ToList();

        var scoresToPersist = new List<AssessmentAnswerScore>();
        var updatedReviewAssignments = new List<ReviewAssignment>();

        // 2. Iterate through each FillAssignment present in the submission
        foreach (var assignmentGroup in answersByAssignment)
        {
            int fillAssignmentId = assignmentGroup.Key;
            var groupAnswerIds = assignmentGroup.Select(a => a.Id).ToHashSet();

            // Validate FillAssignment exists and is completed
            var fillAssignment = await assignmentRepository.GetAssignmentByIdAsync(fillAssignmentId, cancellationToken);
            if (fillAssignment == null || fillAssignment.IsDeleted)
            {
                return Result.Failure<int>($"Penugasan pengisian ID {fillAssignmentId} tidak ditemukan.");
            }

            string fillStatus = fillAssignment.Status?.ToLower() ?? "";
            if (!(fillStatus == "on review" || fillStatus == "waiting for review"))
            {
                return Result.Failure<int>($"Penugasan ID {fillAssignmentId} belum diselesaikan oleh pegawai sehingga belum dapat dinilai.");
            }

            // Verify all text answers for this FillAssignment are answered if submitting final (not draft)
            if (!req.IsDraft)
            {
                var validTextAnswers = await assignmentRepository.GetTextAnswersForAssignmentAsync(fillAssignmentId, cancellationToken);
                var validAnswerIds = validTextAnswers.Select(a => a.Id).ToHashSet();

                var submittedForThisAssignment = groupAnswerIds.Where(id => validAnswerIds.Contains(id)).ToList();
                if (submittedForThisAssignment.Count != validAnswerIds.Count)
                {
                    return Result.Failure<int>($"Semua ({validAnswerIds.Count}) jawaban teks untuk penugasan ID {fillAssignmentId} wajib diberi nilai.");
                }
            }

            // Resolve or Create ReviewAssignment record
            var reviewAssignment = await assignmentRepository.GetReviewAssignmentAsync(
                req.PlanId,
                req.IntervalId,
                fillAssignmentId,
                reviewerId,
                cancellationToken);

            if (reviewAssignment == null)
            {
                // First time submitting: Create brand new ReviewAssignment and auto-claim
                string initialStatus = req.IsDraft ? "drafted" : "in progress";

                reviewAssignment = new ReviewAssignment(
                    req.PlanId,
                    req.IntervalId,
                    fillAssignmentId,
                    req.ReviewerRoleId,
                    req.ReviewerJobTitle,
                    reviewerId,
                    initialStatus,
                    reviewerId
                );

                await assignmentRepository.AddReviewAssignmentAsync(reviewAssignment, cancellationToken);
                // Commit to ensure reviewAssignment.Id is generated for dependent scores
                await unitOfWork.CommitAsync(cancellationToken);
            }
            else if (!reviewAssignment.ReviewerId.HasValue)
            {
                // Unclaimed review template exists: Claim it
                reviewAssignment.ClaimBySupervisor(reviewerId, reviewerId);
            }
            else if (reviewAssignment.ReviewerId != reviewerId)
            {
                // Claimed by another supervisor
                return Result.Failure<int>($"Akses ditolak: Penugasan ID {fillAssignmentId} sudah diklaim oleh supervisor lain.");
            }

            // Prevent modification if already finalized
            string currentStatus = reviewAssignment.Status?.ToLower() ?? "not started";
            if (currentStatus == "done" || currentStatus == "submitted")
            {
                return Result.Failure<int>($"Penilaian untuk penugasan ID {fillAssignmentId} sudah dikirimkan sebelumnya dan tidak dapat diubah.");
            }

            // 3. Score validation and entity construction
            foreach (var answer in assignmentGroup)
            {
                var scorePayload = req.ReviewScores.First(r => r.AssessmentAnswerId == answer.Id);

                if (!req.IsDraft && (scorePayload.Score < 0 || scorePayload.Score > 100))
                {
                    return Result.Failure<int>($"Skor untuk jawaban ID {answer.Id} harus berada di antara 0 dan 100.");
                }

                scoresToPersist.Add(new AssessmentAnswerScore(
                    reviewAssignment.Id,
                    answer.Id,
                    scorePayload.Score,
                    reviewerId,
                    reviewerId
                ));
            }

            // Update ReviewAssignment status
            string targetStatus = req.IsDraft ? "drafted" : "submitted";
            reviewAssignment.ApplyUpdate(targetStatus, reviewerId);
            updatedReviewAssignments.Add(reviewAssignment);
        }

        // 4. Persist Scores & Finalize Statuses in a single batch transaction
        await assignmentRepository.UpsertAnswerScoresAsync(scoresToPersist, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(updatedReviewAssignments.FirstOrDefault()?.Id ?? 0);
    }
}