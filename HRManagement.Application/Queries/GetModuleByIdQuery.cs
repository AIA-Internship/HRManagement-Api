using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningMapping;
using MediatR;
using System.Linq;

namespace HRManagement.Application.Queries
{
    public class GetModuleByIdQuery(int moduleId) : IRequest<Result<ReadModuleDetailDto>>
    {
        public int ModuleId { get; set; } = moduleId;
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
                var quiz = await _repo.GetQuizByModuleIdAsync(request.ModuleId);

                ReadQuizSummaryDto? quizDto = null;
                if (quiz != null)
                {
                    var questionCount = await _repo.GetQuestionCountByQuizIdAsync(quiz.QuizId);
                    quizDto = new ReadQuizSummaryDto
                    {
                        quizId = quiz.QuizId,
                        questionCount = questionCount,
                        mcCount = quiz.McCount,
                        essayCount = quiz.EssayCount,
                        mcWeight = quiz.McWeight,
                        essayWeight = quiz.EssayWeight,
                        minimumPassingScore = quiz.MinimumPassingScore
                    };
                }

                var dto = new ReadModuleDetailDto
                {
                    moduleId = m.ModuleId,
                    title = m.ModuleTitle,
                    description = m.ModuleDescription,
                    role = m.TargetRole,
                    dueDate = m.DueDate,
                    createdUtcDate = m.CreatedUtcDate,
                    contents = contents.Select(ModuleContentMapping.MapToReadDto).ToList(),
                    quiz = quizDto
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
