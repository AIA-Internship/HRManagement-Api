using CSharpFunctionalExtensions;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
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
    public class CreateQuizCommand(CreateQuizConfigurationDto dto) : IRequest<Result<int>>
    {
        public CreateQuizConfigurationDto Dto { get; set; } = dto;
    }

    internal class CreateQuizHandler : IRequestHandler<CreateQuizCommand, Result<int>>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CreateQuizHandler> _logger;

        public CreateQuizHandler(AppDbContext context, ILogger<CreateQuizHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(CreateQuizCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CreateQuizHandler));

            var dto = request.Dto;

            if (dto.mcWeight + dto.essayWeight != 100)
                return Result.Failure<int>("MC weight and essay weight must add up to 100%.");

            var mcQuestions = dto.questions.Where(q => q.questionType == "MC").ToList();
            var essayQuestions = dto.questions.Where(q => q.questionType == "Essay").ToList();

            if (mcQuestions.Count != dto.mcCount)
                return Result.Failure<int>($"Expected {dto.mcCount} multiple choice question(s), received {mcQuestions.Count}.");

            if (essayQuestions.Count != dto.essayCount)
                return Result.Failure<int>($"Expected {dto.essayCount} essay question(s), received {essayQuestions.Count}.");

            foreach (var mc in mcQuestions)
            {
                if (mc.options.Count != 4)
                    return Result.Failure<int>($"Question \"{mc.questionText}\" must have exactly 4 options (A, B, C, D).");

                var letters = mc.options.Select(o => o.optionLetter.Trim().ToUpperInvariant()).ToList();
                if (!new HashSet<string>(letters).SetEquals(new[] { "A", "B", "C", "D" }))
                    return Result.Failure<int>($"Question \"{mc.questionText}\" options must be labeled A, B, C, and D.");

                if (mc.options.Count(o => o.isCorrect) != 1)
                    return Result.Failure<int>($"Question \"{mc.questionText}\" must have exactly one correct option.");
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                    var module = await _context.ELearningModules
                        .FirstOrDefaultAsync(m => m.ModuleId == dto.moduleId && !m.IsDeleted, ct);
                    if (module == null) return Result.Failure<int>("Module not found.");


                    var quiz = new QuizModel
                    {
                        ModuleId = dto.moduleId,
                        McCount = dto.mcCount,
                        EssayCount = dto.essayCount,
                        McWeight = dto.mcWeight,
                        EssayWeight = dto.essayWeight,
                        MinimumPassingScore = dto.minimumPassingScore,
                        CreatedBy = dto.currentUserId.ToString()
                    };
                    _context.ELearningQuizzes.Add(quiz);
                    await _context.SaveChangesAsync(ct);

                    foreach (var q in dto.questions.OrderBy(q => q.sortOrder))
                    {
                        var question = new QuizQuestionModel
                        {
                            QuizId = quiz.QuizId,
                            QuestionText = q.questionText,
                            QuestionType = q.questionType,
                            SortOrder = q.sortOrder
                        };
                        _context.ELearningQuizQuestions.Add(question);
                        await _context.SaveChangesAsync(ct);

                        foreach (var o in q.options)
                        {
                            _context.ELearningQuizQuestionOptions.Add(new QuizQuestionOptionModel
                            {
                                QuestionId = question.QuestionId,
                                OptionLetter = o.optionLetter,
                                OptionText = o.optionText,
                                IsCorrect = o.isCorrect
                            });
                        }
                    }
                    await _context.SaveChangesAsync(ct);

                    await transaction.CommitAsync(ct);
                    return Result.Success(quiz.QuizId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(ct);
                    _logger.LogError(ex, "Error creating quiz for module {moduleId}", dto.moduleId);
                    return Result.Failure<int>(ex.Message);
                }
            });
        }
    }
}
