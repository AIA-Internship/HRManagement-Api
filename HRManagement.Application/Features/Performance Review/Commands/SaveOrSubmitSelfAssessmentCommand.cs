using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Commands;

public record SaveOrSubmitSelfAssessmentCommand(
    int AssignmentId,
    SaveOrSubmitSelfAssessmentPayload Payload,
    int CurrentUserId
) : IRequest<Result>;

internal sealed class SaveOrSubmitSelfAssessmentCommandHandler(
    IAssignmentRepository assignmentRepository,
    ILogger<SaveOrSubmitSelfAssessmentCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<SaveOrSubmitSelfAssessmentCommand, Result>
{
    public async Task<Result> Handle(SaveOrSubmitSelfAssessmentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(SaveOrSubmitSelfAssessmentCommandHandler));

        var payload = request.Payload;
        var currentUserId = request.CurrentUserId;
        var assignmentId = request.AssignmentId;

        var assignmentDetail = await assignmentRepository.GetAssignmentForValidationAsync(assignmentId, cancellationToken);
        if (assignmentDetail == null)
        {
            return Result.Failure("Penugasan penilaian diri tidak ditemukan atau siklus evaluasi tidak aktif.");
        }
        if (assignmentDetail.FillerId != currentUserId)
        {
            logger.LogWarning("User {UserId} attempted unauthorized access to assignment {AssignmentId}", currentUserId, assignmentId);
            return Result.Failure("Akses ditolak: Anda hanya dapat mengisi penilaian mandiri untuk akun Anda sendiri.");
        }

        string status = assignmentDetail.Status ?? AssignmentStatusConstant.NotStarted;
        bool isAlreadyFinalized = string.Equals(status, AssignmentStatusConstant.Completed, StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(status, AssignmentStatusConstant.OnReview, StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(status, AssignmentStatusConstant.PendingReview, StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(status, AssignmentStatusConstant.Reviewed, StringComparison.OrdinalIgnoreCase);

        if (isAlreadyFinalized)
        {
            return Result.Failure("Penilaian mandiri ini sudah dikirimkan sebelumnya dan tidak dapat diubah kembali.");
        }

        // 1. Extract valid questions map
        var validQuestions = assignmentDetail.ValidQuestions;
        var allowedQuestionIds = validQuestions.Keys.ToHashSet();

        if (!payload.IsDraft)
        {
            var distinctIncomingQuestionIds = payload.Answers
                .Select(a => a.AssessmentQuestionId)
                .Distinct()
                .ToHashSet();

            if (distinctIncomingQuestionIds.Count != allowedQuestionIds.Count ||
                distinctIncomingQuestionIds.Any(id => !allowedQuestionIds.Contains(id)))
            {
                return Result.Failure($"Semua ({allowedQuestionIds.Count}) pertanyaan wajib diisi sebelum dikirim.");
            }
        }

        var answersToPersist = new List<AssessmentAnswer>();

        foreach (var answerDto in payload.Answers)
        {
            if (!validQuestions.TryGetValue(answerDto.AssessmentQuestionId, out var qType))
            {
                return Result.Failure($"Pertanyaan ID {answerDto.AssessmentQuestionId} tidak valid untuk penugasan ini.");
            }

            // Resolve question type with fallback to assignment answer type
            string questionType = !string.IsNullOrWhiteSpace(qType)
                ? qType.ToLower()
                : (assignmentDetail.AnswerType?.ToLower() ?? AssessmentAnswerTypeConstants.Text);

            if (!payload.IsDraft)
            {
                // Flexible per-question validation
                bool requiresRating = questionType.Contains(AssessmentAnswerTypeConstants.Rating);
                bool requiresText = questionType.Contains(AssessmentAnswerTypeConstants.Text);

                if (requiresRating && (!answerDto.RatingValue.HasValue || answerDto.RatingValue < 1 || answerDto.RatingValue > 5))
                {
                    return Result.Failure($"Pertanyaan ID {answerDto.AssessmentQuestionId} memerlukan penilaian angka antara 1 sampai 5.");
                }

                if (requiresText && string.IsNullOrWhiteSpace(answerDto.TextValue))
                {
                    return Result.Failure($"Feedback naratif wajib diisi untuk pertanyaan ID {answerDto.AssessmentQuestionId} sebelum dikirim.");
                }
            }

            // Pass raw values directly; UpsertAnswersAsync handles value sanitization via ApplyUpdate
            answersToPersist.Add(new AssessmentAnswer(
                assignmentId,
                answerDto.AssessmentQuestionId,
                answerDto.TextValue,
                answerDto.RatingValue,
                currentUserId
            ));
        }

        var assignmentAnswerTypes = new Dictionary<int, string> { { assignmentId, assignmentDetail.AnswerType } };
        await assignmentRepository.UpsertAnswersAsync(answersToPersist, assignmentAnswerTypes, cancellationToken);

        var assignments = await assignmentRepository.GetAssignmentsWithAssessmentsAsync(
            new List<int> { assignmentId },
            cancellationToken
        );

        var assignmentToUpdate = assignments.FirstOrDefault();
        if (assignmentToUpdate != null)
        {
            // 👉 Pass the metadata explicitly retrieved from assignmentDetail
            assignmentToUpdate.FinalizeSubmission(
                payload.IsDraft,
                assignmentDetail.AssessmentType,
                assignmentDetail.AnswerType,
                currentUserId
            );
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}