using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using HRManagement.MsSQL.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Commands.ELearningCommands
{
    public class CopyModuleCommand(CopyModuleDto dto) : IRequest<Result>
    {
        public CopyModuleDto Dto { get; set; } = dto;
    }

    internal class CopyModuleHandler : IRequestHandler<CopyModuleCommand, Result>
    {
        private readonly ILogger<CopyModuleHandler> _logger;
        private readonly AppDbContext _context;

        public CopyModuleHandler(AppDbContext context, ILogger<CopyModuleHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result> Handle(CopyModuleCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CopyModuleHandler));

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                var sourceModule = await _context.ELearningModules
                    .FirstOrDefaultAsync(m => m.ModuleId == request.Dto.sourceModuleId && !m.IsDeleted, ct);

                if (sourceModule == null)
                    return Result.Failure("Source module not found.");

                var clonedModule = new ModuleModel
                {
                    BatchId = request.Dto.targetBatchId,
                    ModuleTitle = sourceModule.ModuleTitle,
                    ModuleDescription = sourceModule.ModuleDescription,
                    TargetRole = sourceModule.TargetRole,
                    DueDate = sourceModule.DueDate,
                    IsDeleted = false,
                    CreatedBy = request.Dto.currentUserId.ToString(),
                    CreatedUtcDate = DateTime.UtcNow
                };

                _context.ELearningModules.Add(clonedModule);
                await _context.SaveChangesAsync(ct);

                var sourceContents = await _context.ELearningModuleContents
                    .Where(c => c.ModuleId == request.Dto.sourceModuleId && !c.IsDeleted)
                    .ToListAsync(ct);

                if (sourceContents.Any())
                {
                    foreach (var content in sourceContents)
                    {
                        var clonedContent = new ModuleContentModel
                        {
                            ModuleId = clonedModule.ModuleId,
                            ContentTitle = content.ContentTitle,
                            ContentType = content.ContentType,
                            ContentUrl = content.ContentUrl,
                            SortOrder = content.SortOrder,
                            IsDeleted = false,
                            CreatedBy = request.Dto.currentUserId.ToString(),
                            CreatedUtcDate = DateTime.UtcNow
                        };
                        _context.ELearningModuleContents.Add(clonedContent);
                    }
                    await _context.SaveChangesAsync(ct);
                }

                var sourceQuiz = await _context.ELearningQuizzes
                    .FirstOrDefaultAsync(q => q.ModuleId == request.Dto.sourceModuleId && !q.IsDeleted, ct);

                if (sourceQuiz != null)
                {
                    var clonedQuiz = new QuizModel
                    {
                        ModuleId = clonedModule.ModuleId,
                        McCount = sourceQuiz.McCount,
                        EssayCount = sourceQuiz.EssayCount,
                        McWeight = sourceQuiz.McWeight,
                        EssayWeight = sourceQuiz.EssayWeight,
                        MinimumPassingScore = sourceQuiz.MinimumPassingScore,
                        IsDeleted = false,
                        CreatedBy = request.Dto.currentUserId.ToString(),
                        CreatedUtcDate = DateTime.UtcNow
                    };
                    _context.ELearningQuizzes.Add(clonedQuiz);
                    await _context.SaveChangesAsync(ct);

                    var sourceQuestions = await _context.ELearningQuizQuestions
                        .Where(q => q.QuizId == sourceQuiz.QuizId)
                        .OrderBy(q => q.SortOrder)
                        .ToListAsync(ct);

                    foreach (var question in sourceQuestions)
                    {
                        var clonedQuestion = new QuizQuestionModel
                        {
                            QuizId = clonedQuiz.QuizId,
                            QuestionText = question.QuestionText,
                            QuestionType = question.QuestionType,
                            SortOrder = question.SortOrder
                        };
                        _context.ELearningQuizQuestions.Add(clonedQuestion);
                        await _context.SaveChangesAsync(ct);

                        var sourceOptions = await _context.ELearningQuizQuestionOptions
                            .Where(o => o.QuestionId == question.QuestionId)
                            .ToListAsync(ct);

                        foreach (var option in sourceOptions)
                        {
                            _context.ELearningQuizQuestionOptions.Add(new QuizQuestionOptionModel
                            {
                                QuestionId = clonedQuestion.QuestionId,
                                OptionLetter = option.OptionLetter,
                                OptionText = option.OptionText,
                                IsCorrect = option.IsCorrect
                            });
                        }
                    }
                    await _context.SaveChangesAsync(ct);
                }

                await transaction.CommitAsync(ct);
                return Result.Success();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(ct);
                    _logger.LogError(ex, "Error copying module id {sourceId}", request.Dto.sourceModuleId);
                    return Result.Failure(ex.Message);
                }
            });
        }
    }
}