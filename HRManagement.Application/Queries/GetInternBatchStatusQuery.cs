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
    public class GetInternBatchStatusQuery(int employeeId, int batchId) : IRequest<Result<ReadInternBatchStatusDto>>
    {
        public int EmployeeId { get; set; } = employeeId;
        public int BatchId { get; set; } = batchId;
    }

    internal class GetInternBatchStatusHandler : IRequestHandler<GetInternBatchStatusQuery, Result<ReadInternBatchStatusDto>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetInternBatchStatusHandler> _logger;

        public GetInternBatchStatusHandler(IELearningRepository repo, ILogger<GetInternBatchStatusHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ReadInternBatchStatusDto>> Handle(GetInternBatchStatusQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetInternBatchStatusHandler));
            try
            {
                var batch = await _repo.GetBatchByIdAsync(request.BatchId);
                if (batch == null) return Result.Failure<ReadInternBatchStatusDto>("Batch not found.");

                var allEmployeeModules = await _repo.GetModulesByEmployeeCohortAsync(request.EmployeeId, "");
                var modules = allEmployeeModules.Where(m => m.BatchId == request.BatchId).ToList();

                var progressRecords = (await _repo.GetProgressRecordsByEmployeeAsync(request.EmployeeId)).ToList();
                var openedContentIds = new HashSet<int>(await _repo.GetOpenedContentIdsByEmployeeAsync(request.EmployeeId));

                int finishedCount = 0;
                int missedDueDateCount = 0;

                foreach (var module in modules)
                {
                    var progress = progressRecords.FirstOrDefault(p => p.ModuleId == module.ModuleId);
                    bool isCompleted = progress != null && progress.ProgressStatus == "Completed";

                    if (isCompleted)
                    {
                        finishedCount++;
                    }
                    else if (module.DueDate.HasValue && DateTime.UtcNow.Date > module.DueDate.Value.Date)
                    {
                        missedDueDateCount++;
                    }
                }

                int totalModules = modules.Count;
                string status;

                if (totalModules > 0 && finishedCount == totalModules)
                {
                    status = "Completed";
                }
                else if (totalModules > 0 && missedDueDateCount > (totalModules / 2.0))
                {
                    status = "Out of Track";
                }
                else
                {
                    status = "On track";
                }

                var result = new ReadInternBatchStatusDto
                {
                    employeeId = request.EmployeeId,
                    batchId = request.BatchId,
                    totalModules = totalModules,
                    finishedModules = finishedCount,
                    status = status
                };

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching batch status for employee {employeeId} in batch {batchId}", request.EmployeeId, request.BatchId);
                return Result.Failure<ReadInternBatchStatusDto>(ex.Message);
            }
        }
    }
}
