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
    public class GetInternModuleProgressQuery(int employeeId, int programId) : IRequest<Result<ApiResponse>>
    {
        public int EmployeeId { get; set; } = employeeId;
        public int ProgramId { get; set; } = programId;
    }

    internal class GetInternModuleProgressHandler : IRequestHandler<GetInternModuleProgressQuery, Result<ApiResponse>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetInternModuleProgressHandler> _logger;

        public GetInternModuleProgressHandler(IELearningRepository repo, ILogger<GetInternModuleProgressHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(GetInternModuleProgressQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetInternModuleProgressHandler));
            try
            {
                var modules = await _repo.GetModulesByProgramIdAsync(request.ProgramId);
                var progressRecords = await _repo.GetProgressRecordsByEmployeeAsync(request.EmployeeId);

                var result = modules.Select(m =>
                {
                    var progress = progressRecords.FirstOrDefault(p => p.ModuleId == m.ModuleId);
                    return new
                    {
                        moduleId = m.ModuleId,
                        title = m.ModuleTitle,
                        role = m.TargetRole,
                        dueDate = m.DueDate,
                        progressStatus = progress?.ProgressStatus ?? "Not Started"
                    };
                })
                .OrderBy(m => m.title)
                .ToList();

                return ApiHelperResponse.Success("Intern module progress retrieved", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching module progress for employee {employeeId} in program {programId}", request.EmployeeId, request.ProgramId);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
