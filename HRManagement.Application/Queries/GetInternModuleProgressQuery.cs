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
    public class GetInternModuleProgressQuery(int employeeId, int programId) : IRequest<Result<List<ReadInternModuleProgressDto>>>
    {
        public int EmployeeId { get; set; } = employeeId;
        public int ProgramId { get; set; } = programId;
    }

    internal class GetInternModuleProgressHandler : IRequestHandler<GetInternModuleProgressQuery, Result<List<ReadInternModuleProgressDto>>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetInternModuleProgressHandler> _logger;

        public GetInternModuleProgressHandler(IELearningRepository repo, ILogger<GetInternModuleProgressHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<List<ReadInternModuleProgressDto>>> Handle(GetInternModuleProgressQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetInternModuleProgressHandler));
            try
            {
                var modules = await _repo.GetModulesByProgramIdAsync(request.ProgramId);
                var progressRecords = await _repo.GetProgressRecordsByEmployeeAsync(request.EmployeeId);

                var result = modules.Select(m =>
                {
                    var progress = progressRecords.FirstOrDefault(p => p.ModuleId == m.ModuleId);
                    return new ReadInternModuleProgressDto
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

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching module progress for employee {employeeId} in program {programId}", request.EmployeeId, request.ProgramId);
                return Result.Failure<List<ReadInternModuleProgressDto>>(ex.Message);
            }
        }
    }
}
