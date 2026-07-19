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

                var modules = (await _repo.GetModulesByBatchIdAsync(request.BatchId, "", new List<string>())).ToList();
                var progressRecords = (await _repo.GetProgressRecordsByEmployeeAsync(request.EmployeeId)).ToList();
                var openedContentIds = new HashSet<int>(await _repo.GetOpenedContentIdsByEmployeeAsync(request.EmployeeId));

                int finishedCount = 0;

                foreach (var module in modules)
                {
                    var contents = await _repo.GetContentsByModuleIdAsync(module.ModuleId);
                    bool allContentsOpened = contents.All(c => openedContentIds.Contains(c.ContentId));

                    var quiz = await _repo.GetQuizByModuleIdAsync(module.ModuleId);
                    var progress = progressRecords.FirstOrDefault(p => p.ModuleId == module.ModuleId);
                    bool quizPassed = quiz == null || progress?.ProgressStatus == "Completed";

                    if (allContentsOpened && quizPassed)
                        finishedCount++;
                }

                int totalModules = modules.Count;
                string status;

                if (DateTime.UtcNow < batch.EndDate)
                {
                    status = "In Progress";
                }
                else
                {
                    double completionRate = totalModules > 0 ? (double)finishedCount / totalModules : 0;
                    status = completionRate > 0.5 ? "Completed" : "Failed";
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
