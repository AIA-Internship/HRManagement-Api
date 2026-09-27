using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Commands;

public record SaveOrSubmitPeerReviewCommand(
    int IntervalId,
    SaveOrSubmitPeerReviewPayload Payload,
    int CurrentUserId
) : IRequest<Result>;

internal sealed class SaveOrSubmitPeerReviewCommandHandler(
    IAssignmentRepository assignmentRepository,
    ILogger<SaveOrSubmitPeerReviewCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<SaveOrSubmitPeerReviewCommand, Result>
{
    public async Task<Result> Handle(SaveOrSubmitPeerReviewCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(SaveOrSubmitPeerReviewCommandHandler));

        var payload = request.Payload;
        var currentUserId = request.CurrentUserId;
        var intervalId = request.IntervalId;

        var validAssignments = await assignmentRepository.GetAssignmentsForValidationAsync(intervalId, currentUserId, cancellationToken);
        var validAssignmentIds = validAssignments.Select(a => a.Id).ToHashSet();
        if (!validAssignments.Any())
        {
            return Result.Failure("Tidak ada penugasan penilaian sejawat yang aktif ditemukan untuk siklus ini.");
        }

        var incomingAssignmentIds = payload.PeerReviews.Select(pr => pr.AssignmentId);
        if (incomingAssignmentIds.Any(id => !validAssignmentIds.Contains(id)))
        {
            return Result.Failure("Akses ditolak: Payload mengandung data penugasan yang tidak valid.");
        }

        var assignmentsToProcess = validAssignments.Where(a => incomingAssignmentIds.Contains(a.Id));
        foreach (var assignment in assignmentsToProcess)
        {
            string status = assignment.Status?.ToLower() ?? "not started";
            if (status == "done" || status == "on review" || status == "submitted")
            {
                return Result.Failure($"Penilaian untuk beberapa rekan kerja sudah dikirimkan sebelumnya dan tidak dapat diubah kembali.");
            }
        }

        var answersToPersist = new List<AssessmentAnswer>();

        foreach (var peerReviewDto in payload.PeerReviews)
        {
            var matchedAssignment = validAssignments.First(a => a.Id == peerReviewDto.AssignmentId);
            if (matchedAssignment.Status?.Equals("Submitted", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Result.Failure($"Penugasan ID {matchedAssignment.Id} sudah dikirim dan tidak dapat diubah.");
            }

            // CHANGED: Use ValidQuestions dictionary
            var validQuestions = matchedAssignment.ValidQuestions;
            var allowedQuestionIds = validQuestions.Keys.ToHashSet();

            if (!payload.IsDraft)
            {
                var distinctIncomingQuestionIds = peerReviewDto.Answers
                    .Select(a => a.AssessmentQuestionId)
                    .Distinct()
                    .ToHashSet();

                if (distinctIncomingQuestionIds.Count != allowedQuestionIds.Count ||
                    distinctIncomingQuestionIds.Any(id => !allowedQuestionIds.Contains(id)))
                {
                    return Result.Failure($"Semua ({allowedQuestionIds.Count}) pertanyaan wajib diisi sebelum dikirim.");
                }
            }

            foreach (var answerDto in peerReviewDto.Answers)
            {
                if (!validQuestions.TryGetValue(answerDto.AssessmentQuestionId, out var qType))
                {
                    return Result.Failure($"Pertanyaan ID {answerDto.AssessmentQuestionId} tidak valid untuk penugasan ini.");
                }

                // Resolve question type (fallback to assessment-level type)
                string questionType = !string.IsNullOrWhiteSpace(qType)
                    ? qType.ToLower()
                    : (matchedAssignment.AnswerType?.ToLower() ?? "text");

                if (!payload.IsDraft)
                {
                    // CHANGED: Flexible per-question type checks
                    bool requiresRating = questionType.Contains("rating");
                    bool requiresText = questionType.Contains("text");

                    if (requiresRating && (!answerDto.RatingValue.HasValue || answerDto.RatingValue < 1 || answerDto.RatingValue > 5))
                    {
                        return Result.Failure($"Pertanyaan ID {answerDto.AssessmentQuestionId} memerlukan penilaian angka antara 1 sampai 5.");
                    }

                    if (requiresText && string.IsNullOrWhiteSpace(answerDto.TextValue))
                    {
                        return Result.Failure($"Feedback naratif wajib diisi untuk pertanyaan ID {answerDto.AssessmentQuestionId} sebelum dikirim.");
                    }
                }

                // CHANGED: Pass raw values directly. UpsertAnswersAsync handles value sanitization.
                answersToPersist.Add(new AssessmentAnswer(
                    peerReviewDto.AssignmentId,
                    answerDto.AssessmentQuestionId,
                    answerDto.TextValue,
                    answerDto.RatingValue,
                    currentUserId
                ));
            }
        }

        var assignmentAnswerTypes = validAssignments.ToDictionary(a => a.Id, a => a.AnswerType);
        await assignmentRepository.UpsertAnswersAsync(answersToPersist, assignmentAnswerTypes, cancellationToken);

        var assignmentIdsToUpdate = assignmentsToProcess.Select(a => a.Id).ToList();
        var assignments = await assignmentRepository.GetAssignmentsWithAssessmentsAsync(
            assignmentIdsToUpdate,
            cancellationToken
        );
        foreach (var assignment in assignments)
        {
            assignment.FinalizeSubmission(payload.IsDraft, currentUserId);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}