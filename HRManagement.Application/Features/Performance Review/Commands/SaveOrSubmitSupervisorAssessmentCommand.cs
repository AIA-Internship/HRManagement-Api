using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Commands;

public record SaveOrSubmitSupervisorAssessmentCommand(
    SaveOrSubmitSupervisorAssessmentPayload request,
    int CurrentUserId
) : IRequest<Result<int>>;

internal sealed class SaveOrSubmitSupervisorAssessmentCommandHandler(
    IAssignmentRepository assignmentRepository,
    ILogger<SaveOrSubmitSupervisorAssessmentCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<SaveOrSubmitSupervisorAssessmentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SaveOrSubmitSupervisorAssessmentCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler: {HandlerName}", nameof(SaveOrSubmitSupervisorAssessmentCommandHandler));

        var req = command.request;
        var currentUserId = command.CurrentUserId;
        int resolvedAssignmentId = req.AssignmentId ?? 0;

        FillAssignment? assignmentEntity = null;

        // 1. Resolve or Create Assignment Record
        if (resolvedAssignmentId > 0)
        {
            assignmentEntity = await assignmentRepository.GetAssignmentByIdAsync(resolvedAssignmentId, cancellationToken);
        }
        else
        {
            assignmentEntity = await assignmentRepository.FindAssignmentAsync(
                req.PlanId,
                req.IntervalId,
                currentUserId,
                req.SubjectId,
                req.AssessmentId,
                cancellationToken);
        }

        if (assignmentEntity == null)
        {
            string initialStatus = req.IsDraft ? "drafted" : "in progress";

            assignmentEntity = new FillAssignment(
                req.PlanId,
                req.IntervalId,
                currentUserId,
                req.SubjectId,
                req.AssessmentId,
                initialStatus,
                currentUserId
            );

            await assignmentRepository.AddAssignmentAsync(assignmentEntity, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            resolvedAssignmentId = assignmentEntity.Id;
        }
        else
        {
            resolvedAssignmentId = assignmentEntity.Id;
        }

        // 2. Validate Assignment Ownership and Status
        var assignmentDetail = await assignmentRepository.GetAssignmentForValidationAsync(resolvedAssignmentId, cancellationToken);
        if (assignmentDetail == null)
        {
            return Result.Failure<int>("Penugasan penilaian atasan tidak ditemukan atau siklus evaluasi tidak aktif.");
        }

        if (assignmentDetail.FillerId != currentUserId)
        {
            logger.LogWarning("User {UserId} attempted unauthorized access to assignment {AssignmentId}", currentUserId, resolvedAssignmentId);
            return Result.Failure<int>("Akses ditolak: Anda hanya dapat mengisi penilaian atasan untuk penugasan Anda sendiri.");
        }

        string status = assignmentDetail.Status?.ToLower() ?? "not started";
        if (status == "done" || status == "on review" || status == "submitted")
        {
            return Result.Failure<int>("Penilaian atasan ini sudah dikirimkan sebelumnya dan tidak dapat diubah kembali.");
        }

        // 3. Validate Answer Submissions against Assessment Template
        // CHANGED: validQuestions should ideally be Dictionary<int, string> (QuestionId -> QuestionType)
        // If ValidQuestionIds is still a HashSet<int>, retrieve QuestionTypes via repository
        var validQuestions = assignmentDetail.ValidQuestions; // Key: QuestionId, Value: QuestionType
        var allowedQuestionIds = validQuestions.Keys.ToHashSet();

        if (!req.IsDraft)
        {
            var incomingQuestionIds = req.Answers
                .Select(a => a.AssessmentQuestionId)
                .Distinct()
                .ToHashSet();

            if (incomingQuestionIds.Count != allowedQuestionIds.Count ||
                incomingQuestionIds.Any(id => !allowedQuestionIds.Contains(id)))
            {
                return Result.Failure<int>($"Semua ({allowedQuestionIds.Count}) pertanyaan wajib diisi sebelum dikirim.");
            }
        }

        var answersToPersist = new List<AssessmentAnswer>();

        foreach (var answerDto in req.Answers)
        {
            if (!validQuestions.TryGetValue(answerDto.AssessmentQuestionId, out var qType))
            {
                return Result.Failure<int>($"Pertanyaan ID {answerDto.AssessmentQuestionId} tidak valid untuk penugasan ini.");
            }

            // Determine question type fallback to assignment level answer type
            string questionType = !string.IsNullOrWhiteSpace(qType)
                ? qType.ToLower()
                : (assignmentDetail.AnswerType?.ToLower() ?? "text");

            if (!req.IsDraft)
            {
                // CHANGED: Flexible validation based on QuestionType
                bool requiresRating = questionType.Contains("rating");
                bool requiresText = questionType.Contains("text");

                if (requiresRating && (!answerDto.RatingValue.HasValue || answerDto.RatingValue < 1 || answerDto.RatingValue > 5))
                {
                    return Result.Failure<int>($"Pertanyaan ID {answerDto.AssessmentQuestionId} memerlukan penilaian angka antara 1 sampai 5.");
                }

                if (requiresText && string.IsNullOrWhiteSpace(answerDto.TextValue))
                {
                    return Result.Failure<int>($"Feedback naratif wajib diisi untuk pertanyaan ID {answerDto.AssessmentQuestionId} sebelum dikirim.");
                }
            }

            // CHANGED: Pass raw values directly. UpsertAnswersAsync will handle field sanitization.
            answersToPersist.Add(new AssessmentAnswer(
                resolvedAssignmentId,
                answerDto.AssessmentQuestionId,
                answerDto.TextValue,
                answerDto.RatingValue,
                currentUserId
            ));
        }

        // 4. Save Answers and Finalize Status
        var assignmentAnswerTypes = new Dictionary<int, string> { { resolvedAssignmentId, assignmentDetail.AnswerType } };
        await assignmentRepository.UpsertAnswersAsync(answersToPersist, assignmentAnswerTypes, cancellationToken);

        assignmentEntity.FinalizeSubmission(req.IsDraft, currentUserId);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(resolvedAssignmentId);
    }
}