using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
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
    public class GetAvailableModulesQuery(int employeeId, string statusFilter, string search) : IRequest<Result<List<ReadModuleDto>>>
    {
        public int EmployeeId { get; set; } = employeeId;
        public string StatusFilter { get; set; } = statusFilter;
        public string Search { get; set; } = search;
    }

    internal class GetAvailableModulesHandler : IRequestHandler<GetAvailableModulesQuery, Result<List<ReadModuleDto>>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetAvailableModulesHandler> _logger;

        public GetAvailableModulesHandler(IELearningRepository repo, ILogger<GetAvailableModulesHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<List<ReadModuleDto>>> Handle(GetAvailableModulesQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetAvailableModulesHandler));
            try
            {
                var entities = await _repo.GetModulesByEmployeeCohortAsync(request.EmployeeId, request.Search);

                var progressRecords = await _repo.GetProgressRecordsByEmployeeAsync(request.EmployeeId);

                var mappedList = new List<ReadModuleDto>();

                foreach (var m in entities)
                {
                    var userProgress = progressRecords.FirstOrDefault(p => p.ModuleId == m.ModuleId);
                    string currentStatus = userProgress != null ? userProgress.ProgressStatus : "Not Started";

                    if (!request.StatusFilter.Equals("All", StringComparison.OrdinalIgnoreCase) &&
                        !currentStatus.Equals(request.StatusFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var mappedModule = ModuleMapping.MapToReadDto(m);
                    mappedModule.progressStatus = currentStatus;
                    mappedList.Add(mappedModule);
                }

                return Result.Success(mappedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available modules for Employee Id {employeeId}", request.EmployeeId);
                return Result.Failure<List<ReadModuleDto>>(ex.Message);
            }
        }
    }
}