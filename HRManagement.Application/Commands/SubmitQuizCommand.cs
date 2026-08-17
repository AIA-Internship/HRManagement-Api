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
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
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
                    UserId = request.Dto.userId
                };

                _context.Set<QuizSubmissionModel>().Add(executionSubmission);
                await _context.SaveChangesAsync(ct); 

                var entitiesToInsert = new List<StudentAnswerModel>();
                var answersList = request.Dto.answers ?? new List<SubmittedAnswerItemDto>();

                foreach (var studentAns in answersList)
                {
                    if (studentAns == null) continue;
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
                        if (correctOption != null && studentAns.selectedOption != null && 
                            (string.Equals(correctOption.OptionLetter, studentAns.selectedOption, StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(correctOption.OptionText, studentAns.selectedOption, StringComparison.OrdinalIgnoreCase)))
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
                    executionSubmission.GradedUtcDate = DateTime.UtcNow;

                    var existingProgress = await _context.Set<ProgressModel>()
                        .FirstOrDefaultAsync(p => p.EmployeeId == request.Dto.userId && p.ModuleId == quizConfig.ModuleId, ct);

                    var quizzes = await _context.Set<QuizModel>()
                        .Where(q => q.ModuleId == quizConfig.ModuleId && !q.IsDeleted)
                        .ToListAsync(ct);

                    var submissions = await _context.Set<QuizSubmissionModel>()
                        .Where(s => s.UserId == request.Dto.userId)
                        .ToListAsync(ct);

                    bool allPassed = true;
                    foreach (var q in quizzes)
                    {
                        var latestSub = submissions.Where(s => s.QuizId == q.QuizId).OrderByDescending(s => s.CreatedUtcDate).FirstOrDefault();
                        if (q.QuizId == quizConfig.QuizId)
                        {
                            if (finalCalculatedScore < q.MinimumPassingScore)
                            {
                                allPassed = false;
                                break;
                            }
                        }
                        else
                        {
                            if (latestSub == null || latestSub.TotalScore < q.MinimumPassingScore)
                            {
                                allPassed = false;
                                break;
                            }
                        }
                    }

                    var contents = await _context.Set<ModuleContentModel>()
                        .Where(c => c.ModuleId == quizConfig.ModuleId && !c.IsDeleted)
                        .ToListAsync(ct);
                        
                    var openedContents = await _context.Set<ContentProgressModel>()
                        .Where(cp => cp.EmployeeId == request.Dto.userId)
                        .Select(cp => cp.ContentId)
                        .ToListAsync(ct);
                        
                    bool allContentsOpened = contents.All(c => openedContents.Contains(c.ContentId));

                    string finalStatusClassification = (allPassed && allContentsOpened) ? "Completed" : "In Progress";

                    if (existingProgress == null)
                    {
                        _context.Set<ProgressModel>().Add(new ProgressModel
                        {
                            EmployeeId = request.Dto.userId,
                            ModuleId = quizConfig.ModuleId,
                            ProgressStatus = finalStatusClassification
                        });
                    }
                    else
                    {
                        existingProgress.ProgressStatus = finalStatusClassification;
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
                    }
                    else
                    {
                        _context.Set<ProgressModel>().Add(new ProgressModel
                        {
                            EmployeeId = request.Dto.userId,
                            ModuleId = quizConfig.ModuleId,
                            ProgressStatus = "In Progress"
                        });
                    }
                    await _context.SaveChangesAsync(ct);
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
            });
        }
    }
}