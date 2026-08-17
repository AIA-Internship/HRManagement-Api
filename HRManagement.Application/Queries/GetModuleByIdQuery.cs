using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningMapping;
using MediatR;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace HRManagement.Application.Queries
{
    public class GetModuleByIdQuery(int moduleId, int? userId = null) : IRequest<Result<ReadModuleDetailDto>>
    {
        public int ModuleId { get; set; } = moduleId;
        public int? UserId { get; set; } = userId;
    }

    internal class GetModuleByIdHandler : IRequestHandler<GetModuleByIdQuery, Result<ReadModuleDetailDto>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetModuleByIdHandler> _logger;

        public GetModuleByIdHandler(IELearningRepository repo, ILogger<GetModuleByIdHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ReadModuleDetailDto>> Handle(GetModuleByIdQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetModuleByIdHandler));
            try
            {
                var m = await _repo.GetModuleByIdAsync(request.ModuleId);
                if (m == null) return Result.Failure<ReadModuleDetailDto>("Module not found");

                var contents = await _repo.GetContentsByModuleIdAsync(request.ModuleId);
                var quizzes = await _repo.GetQuizzesByModuleIdAsync(request.ModuleId);
                var quizDtos = new List<ReadQuizSummaryDto>();

                foreach (var quiz in quizzes)
                {
                    var questionCount = await _repo.GetQuestionCountByQuizIdAsync(quiz.QuizId);
                    var questions = await _repo.GetQuestionsByQuizIdAsync(quiz.QuizId);
                    var options = await _repo.GetOptionsByQuestionIdsAsync(questions.Select(q => q.QuestionId));

                    decimal? latestScore = null;
                    if (request.UserId.HasValue)
                    {
                        var submissions = await _repo.GetSubmissionsByUserAndQuizIdsAsync(request.UserId.Value, new[] { quiz.QuizId });
                        latestScore = submissions.OrderByDescending(s => s.CreatedUtcDate).FirstOrDefault()?.TotalScore;
                    }

                    var questionDtos = questions.OrderBy(q => q.SortOrder).Select(q => new ReadQuizQuestionDto
                    {
                        id = q.QuestionId,
                        text = q.QuestionText,
                        type = q.QuestionType,
                        options = options.Where(o => o.QuestionId == q.QuestionId).Select(o => o.OptionText).ToList()
                    }).ToList();

                    quizDtos.Add(new ReadQuizSummaryDto
                    {
                        quizId = quiz.QuizId,
                        questionCount = questionCount,
                        mcCount = quiz.McCount,
                        essayCount = quiz.EssayCount,
                        mcWeight = quiz.McWeight,
                        essayWeight = quiz.EssayWeight,
                        minimumPassingScore = quiz.MinimumPassingScore,
                        latestScore = latestScore,
                        questions = questionDtos
                    });
                }

                var contentsList = contents.Select(ModuleContentMapping.MapToReadDto).ToList();
                
                if (request.UserId.HasValue)
                {
                    var openedContentIds = await _repo.GetOpenedContentIdsByUserAndModuleAsync(request.UserId.Value, request.ModuleId);
                    foreach (var contentDto in contentsList)
                    {
                        contentDto.isCompleted = openedContentIds.Contains(contentDto.contentId);
                    }
                }

                var dto = new ReadModuleDetailDto
                {
                    moduleId = m.ModuleId,
                    title = m.ModuleTitle,
                    description = m.ModuleDescription,
                    role = m.TargetRole,
                    dueDate = m.DueDate,
                    createdUtcDate = m.CreatedUtcDate,
                    contents = contentsList,
                    quizzes = quizDtos
                };
                return Result.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching module detail for ID {id}", request.ModuleId);
                return Result.Failure<ReadModuleDetailDto>(ex.Message);
            }
        }
    }
}
