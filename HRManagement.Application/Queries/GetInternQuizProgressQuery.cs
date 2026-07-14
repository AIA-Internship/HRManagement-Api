using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Queries
{
    public class GetInternQuizProgressQuery(int employeeId, int programId) : IRequest<Result<ApiResponse>>
    {
        public int EmployeeId { get; set; } = employeeId;
        public int ProgramId { get; set; } = programId;
    }

    internal class GetInternQuizProgressHandler : IRequestHandler<GetInternQuizProgressQuery, Result<ApiResponse>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetInternQuizProgressHandler> _logger;

        public GetInternQuizProgressHandler(IELearningRepository repo, ILogger<GetInternQuizProgressHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(GetInternQuizProgressQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetInternQuizProgressHandler));
            try
            {
                var quizzes = await _repo.GetQuizzesByProgramIdAsync(request.ProgramId);
                var quizIds = quizzes.Select(q => q.QuizId).ToList();
                var submissions = await _repo.GetSubmissionsByUserAndQuizIdsAsync(request.EmployeeId, quizIds);
                var modules = await _repo.GetModulesByProgramIdAsync(request.ProgramId);

                var result = quizzes.Select(q =>
                {
                    var module = modules.FirstOrDefault(m => m.ModuleId == q.ModuleId);
                    var latestSubmission = submissions
                        .Where(s => s.QuizId == q.QuizId)
                        .OrderByDescending(s => s.CreatedUtcDate)
                        .FirstOrDefault();

                    string status;
                    if (latestSubmission == null)
                        status = "Not Started";
                    else if (latestSubmission.IsPassed == true)
                        status = "Completed";
                    else if (latestSubmission.IsPassed == false)
                        status = "Failed";
                    else
                        status = "In Progress";

                    return new
                    {
                        quizId = q.QuizId,
                        moduleId = q.ModuleId,
                        moduleTitle = module?.ModuleTitle ?? "Unknown",
                        status
                    };
                })
                .OrderBy(q => q.moduleTitle)
                .ToList();

                return ApiHelperResponse.Success("Intern quiz progress retrieved", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quiz progress for employee {employeeId} in program {programId}", request.EmployeeId, request.ProgramId);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
