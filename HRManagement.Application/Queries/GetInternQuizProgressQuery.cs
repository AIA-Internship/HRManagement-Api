using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Queries
{
    public class GetInternQuizProgressQuery(int employeeId, int programId) : IRequest<Result<List<ReadInternQuizProgressDto>>>
    {
        public int EmployeeId { get; set; } = employeeId;
        public int ProgramId { get; set; } = programId;
    }

    internal class GetInternQuizProgressHandler : IRequestHandler<GetInternQuizProgressQuery, Result<List<ReadInternQuizProgressDto>>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetInternQuizProgressHandler> _logger;

        public GetInternQuizProgressHandler(IELearningRepository repo, ILogger<GetInternQuizProgressHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<List<ReadInternQuizProgressDto>>> Handle(GetInternQuizProgressQuery request, CancellationToken ct)
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

                    return new ReadInternQuizProgressDto
                    {
                        quizId = q.QuizId,
                        moduleId = q.ModuleId,
                        moduleTitle = module?.ModuleTitle ?? "Unknown",
                        status = status
                    };
                })
                .OrderBy(q => q.moduleTitle)
                .ToList();

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quiz progress for employee {employeeId} in program {programId}", request.EmployeeId, request.ProgramId);
                return Result.Failure<List<ReadInternQuizProgressDto>>(ex.Message);
            }
        }
    }
}
