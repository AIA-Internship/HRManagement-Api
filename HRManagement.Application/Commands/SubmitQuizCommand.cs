using CSharpFunctionalExtensions;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels;
using HRManagement.MsSQL.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Commands.ELearningCommands
{
    public class SubmitStudentAnswersDto
    {
        public int quizId { get; set; }
        public int userId { get; set; }
        public List<SubmittedAnswerItemDto> answers { get; set; } = new();
    }

    public class SubmittedAnswerItemDto
    {
        public int questionId { get; set; }
        public string? selectedOption { get; set; } 
        public string? essayAnswerText { get; set; } 
    }

    public class SubmitQuizCommand(SubmitStudentAnswersDto dto) : IRequest<Result<int>>
    {
        public SubmitStudentAnswersDto Dto { get; set; } = dto;
    }

    internal class SubmitQuizHandler : IRequestHandler<SubmitQuizCommand, Result<int>>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SubmitQuizHandler> _logger;

        public SubmitQuizHandler(AppDbContext context, ILogger<SubmitQuizHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(SubmitQuizCommand request, CancellationToken ct)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var quizConfig = await _context.Set<QuizModel>()
                    .FirstOrDefaultAsync(q => q.QuizId == request.Dto.quizId, ct);

                if (quizConfig == null) return Result.Failure<int>("Quiz parameters not found.");

                var systemQuestions = await _context.Set<QuizQuestionModel>().Where(q => q.QuizId == request.Dto.quizId).ToListAsync(ct);
                var mcOptions = await _context.Set<QuizQuestionOptionModel>().Where(o => systemQuestions.Select(q => q.QuestionId).Contains(o.QuestionId)).ToListAsync(ct);

                decimal calculatedMcPoints = 0;
                int actualMcQuestionsCount = systemQuestions.Count(q => q.QuestionType == "MC");

                var executionSubmission = new QuizSubmissionModel
                {
                    QuizId = request.Dto.quizId,
                    UserId = request.Dto.userId,
                    CreatedBy = request.Dto.userId.ToString(),
                    CreatedUtcDate = DateTime.UtcNow
                };

                _context.Set<QuizSubmissionModel>().Add(executionSubmission);
                await _context.SaveChangesAsync(ct); 

                var entitiesToInsert = new List<StudentAnswerModel>();

                foreach (var studentAns in request.Dto.answers)
                {
                    var questionRef = systemQuestions.FirstOrDefault(q => q.QuestionId == studentAns.questionId);
                    if (questionRef == null) continue;

                    var structuralRecord = new StudentAnswerModel
                    {
                        SubmissionId = executionSubmission.SubmissionId,
                        QuestionId = studentAns.questionId,
                        SelectedOption = studentAns.selectedOption,
                        EssayAnswerText = studentAns.essayAnswerText,
                        IsEvaluated = questionRef.QuestionType == "MC",
                        AssignedScore = 0
                    };

                    if (questionRef.QuestionType == "MC")
                    {
                        var correctOption = mcOptions.FirstOrDefault(o => o.QuestionId == studentAns.questionId && o.IsCorrect);
                        if (correctOption != null && correctOption.OptionLetter.Equals(studentAns.selectedOption, StringComparison.OrdinalIgnoreCase))
                        {
                            decimal pointPerMc = actualMcQuestionsCount > 0 ? (decimal)100 / actualMcQuestionsCount : 0;
                            structuralRecord.AssignedScore = pointPerMc;
                            calculatedMcPoints += pointPerMc;
                        }
                    }

                    entitiesToInsert.Add(structuralRecord);
                }

                _context.Set<StudentAnswerModel>().AddRange(entitiesToInsert);
                await _context.SaveChangesAsync(ct);

                if (quizConfig.EssayCount == 0)
                {
                    decimal finalCalculatedScore = calculatedMcPoints * ((decimal)quizConfig.McWeight / 100);
                    executionSubmission.TotalScore = finalCalculatedScore;
                    executionSubmission.IsPassed = finalCalculatedScore >= quizConfig.MinimumPassingScore;
                    executionSubmission.GradedBy = "System";
                    executionSubmission.GradedUtcDate = DateTime.UtcNow;

                    var existingProgress = await _context.Set<ProgressModel>()
                        .FirstOrDefaultAsync(p => p.EmployeeId == request.Dto.userId && p.ModuleId == quizConfig.ModuleId, ct);

                    string finalStatusClassification = executionSubmission.IsPassed == true ? "Completed" : "Failed";

                    if (existingProgress == null)
                    {
                        _context.Set<ProgressModel>().Add(new ProgressModel
                        {
                            EmployeeId = request.Dto.userId,
                            ModuleId = quizConfig.ModuleId,
                            ProgressStatus = finalStatusClassification,
                            CreatedUtcDate = DateTime.UtcNow,
                            CreatedBy = request.Dto.userId.ToString()
                        });
                    }
                    else
                    {
                        existingProgress.ProgressStatus = finalStatusClassification;
                        existingProgress.ModifiedUtcDate = DateTime.UtcNow;
                    }
                    await _context.SaveChangesAsync(ct);
                }
                else
                {
                    var existingProgress = await _context.Set<ProgressModel>()
                        .FirstOrDefaultAsync(p => p.EmployeeId == request.Dto.userId && p.ModuleId == quizConfig.ModuleId, ct);

                    if (existingProgress != null)
                    {
                        existingProgress.ProgressStatus = "In Progress";
                        await _context.SaveChangesAsync(ct);
                    }
                }

                await transaction.CommitAsync(ct);
                return Result.Success(executionSubmission.SubmissionId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Failed validating incoming submission stream execution sequence.");
                return Result.Failure<int>(ex.Message);
            }
        }
    }
}