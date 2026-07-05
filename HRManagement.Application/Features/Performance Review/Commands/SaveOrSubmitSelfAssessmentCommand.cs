using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
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
    IFillAssignmentRepository assignmentRepository,
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

        string status = assignmentDetail.Status?.ToLower() ?? "not started";
        if (status == "done" || status == "on review" || assignmentDetail.Status == "Submitted")
        {
            return Result.Failure("Penilaian mandiri ini sudah dikirimkan sebelumnya dan tidak dapat diubah kembali.");
        }

        string trueAnswerType = assignmentDetail.AnswerType.ToLower();
        var allowedQuestionIds = assignmentDetail.ValidQuestionIds;

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
            if (!allowedQuestionIds.Contains(answerDto.AssessmentQuestionId))
            {
                return Result.Failure($"Pertanyaan ID {answerDto.AssessmentQuestionId} tidak valid untuk penugasan ini.");
            }

            if (!payload.IsDraft)
            {
                if (trueAnswerType == "rating" && (!answerDto.RatingValue.HasValue || answerDto.RatingValue < 1 || answerDto.RatingValue > 5))
                {
                    return Result.Failure($"Pertanyaan ID {answerDto.AssessmentQuestionId} memerlukan penilaian angka antara 1 sampai 5.");
                }

                if (trueAnswerType == "text" && string.IsNullOrWhiteSpace(answerDto.TextValue))
                {
                    return Result.Failure($"Feedback naratif wajib diisi untuk semua pertanyaan sebelum dikirim.");
                }
            }

            answersToPersist.Add(new AssessmentAnswer(
                assignmentId,
                answerDto.AssessmentQuestionId,
                trueAnswerType == "text" ? answerDto.TextValue : null,
                trueAnswerType == "rating" ? answerDto.RatingValue : null,
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
            assignmentToUpdate.FinalizeSubmission(payload.IsDraft, currentUserId);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}