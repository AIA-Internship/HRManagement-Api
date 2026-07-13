using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using HRManagement.Api.Repositories.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Commands.ELearningCommands
{
    public class CreateQuizCommand(CreateQuizConfigurationDto dto) : IRequest<Result<ApiResponse>>
    {
        public CreateQuizConfigurationDto Dto { get; set; } = dto;
    }

    internal class CreateQuizHandler : IRequestHandler<CreateQuizCommand, Result<ApiResponse>>
    {
        private readonly SqlDbContext _context;
        private readonly ILogger<CreateQuizHandler> _logger;

        public CreateQuizHandler(SqlDbContext context, ILogger<CreateQuizHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(CreateQuizCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CreateQuizHandler));

            var dto = request.Dto;

            if (dto.mcWeight + dto.essayWeight != 100)
                return ApiHelperResponse.Failed("MC weight and essay weight must add up to 100%.");

            var mcQuestions = dto.questions.Where(q => q.questionType == "MC").ToList();
            var essayQuestions = dto.questions.Where(q => q.questionType == "Essay").ToList();

            if (mcQuestions.Count != dto.mcCount)
                return ApiHelperResponse.Failed($"Expected {dto.mcCount} multiple choice question(s), received {mcQuestions.Count}.");

            if (essayQuestions.Count != dto.essayCount)
                return ApiHelperResponse.Failed($"Expected {dto.essayCount} essay question(s), received {essayQuestions.Count}.");

            foreach (var mc in mcQuestions)
            {
                if (mc.options.Count(o => o.isCorrect) != 1)
                    return ApiHelperResponse.Failed($"Question \"{mc.questionText}\" must have exactly one correct option.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var module = await _context.ELearningModules
                    .FirstOrDefaultAsync(m => m.ModuleId == dto.moduleId && !m.IsDeleted, ct);
                if (module == null) return ApiHelperResponse.Failed("Module not found.");

                var existingQuiz = await _context.ELearningQuizzes
                    .FirstOrDefaultAsync(q => q.ModuleId == dto.moduleId && !q.IsDeleted, ct);
                if (existingQuiz != null) return ApiHelperResponse.Failed("This module already has a quiz.");

                var quiz = new QuizModel
                {
                    ModuleId = dto.moduleId,
                    McCount = dto.mcCount,
                    EssayCount = dto.essayCount,
                    McWeight = dto.mcWeight,
                    EssayWeight = dto.essayWeight,
                    MinimumPassingScore = dto.minimumPassingScore
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
                return ApiHelperResponse.Success("Quiz created successfully", new { quizId = quiz.QuizId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error creating quiz for module {moduleId}", dto.moduleId);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
