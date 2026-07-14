using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningMapping;
using MediatR;
using System.Linq;

namespace HRManagement.Api.Application.Queries
{
    public class GetModuleByIdQuery(int moduleId) : IRequest<Result<ApiResponse>>
    {
        public int ModuleId { get; set; } = moduleId;
    }

    internal class GetModuleByIdHandler : IRequestHandler<GetModuleByIdQuery, Result<ApiResponse>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetModuleByIdHandler> _logger;

        public GetModuleByIdHandler(IELearningRepository repo, ILogger<GetModuleByIdHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(GetModuleByIdQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetModuleByIdHandler));
            try
            {
                var m = await _repo.GetModuleByIdAsync(request.ModuleId);
                if (m == null) return ApiHelperResponse.Failed("Module not found");

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
                    isPriority = m.IsPriority,
                    dueDate = m.DueDate,
                    createdUtcDate = m.CreatedUtcDate,
                    contents = contents.Select(ModuleContentMapping.MapToReadDto).ToList(),
                    quiz = quizDto
                };
                return ApiHelperResponse.Success("Module detail retrieved", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching module detail for ID {id}", request.ModuleId);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
