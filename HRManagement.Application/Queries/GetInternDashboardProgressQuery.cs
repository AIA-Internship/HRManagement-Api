using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using MediatR;
using System;
using System.Linq;

namespace HRManagement.Application.Queries.ELearningQueries
{
    public class GetInternDashboardProgressQuery(int employeeId) : IRequest<Result<ReadDashboardProgressDto>>
    {
        public int EmployeeId { get; set; } = employeeId;
    }

    internal class GetInternDashboardProgressHandler : IRequestHandler<GetInternDashboardProgressQuery, Result<ReadDashboardProgressDto>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetInternDashboardProgressHandler> _logger;
        private readonly ISender _sender;

        public GetInternDashboardProgressHandler(IELearningRepository repo, ILogger<GetInternDashboardProgressHandler> logger, ISender sender)
        {
            _repo = repo;
            _logger = logger;
            _sender = sender;
        }

        public async Task<Result<ReadDashboardProgressDto>> Handle(GetInternDashboardProgressQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetInternDashboardProgressHandler));
            try
            {
                int totalCount = await _repo.GetTotalCohortModulesCountAsync(request.EmployeeId);

                int completedCount = await _repo.GetCompletedCohortModulesCountAsync(request.EmployeeId);

                var today = DateTime.UtcNow.Date;
                var upcomingDeadlines = await _repo.GetUpcomingCohortDeadlinesAsync(request.EmployeeId, today, today.AddDays(7));

                var toDoList = upcomingDeadlines.Select(m => new ReadToDoItemDto
                {
                    moduleId = m.ModuleId,
                    title = m.ModuleTitle,
                    dueDate = m.DueDate,
                    daysLeft = (m.DueDate!.Value.Date - today).Days
                }).ToList();

                var batchesList = await _repo.GetBatchesByEmployeeIdAsync(request.EmployeeId);
                var dashboardBatches = new List<ReadDashboardBatchDto>();

                foreach (var b in batchesList)
                {
                    var batchStatusResult = await _sender.Send(new GetInternBatchStatusQuery(request.EmployeeId, b.BatchId), ct);
                    string status = batchStatusResult.IsSuccess ? batchStatusResult.Value.status : "On track";

                    dashboardBatches.Add(new ReadDashboardBatchDto
                    {
                        id = b.BatchId,
                        name = b.BatchName,
                        period = $"{b.StartDate:dd MMM yyyy} - {b.EndDate:dd MMM yyyy}",
                        endsIn = (b.EndDate.Date - today).Days > 0 ? (b.EndDate.Date - today).Days : 0,
                        status = status
                    });
                }

                var dashboardResult = new ReadDashboardProgressDto
                {
                    totalModules = totalCount,
                    completedModules = completedCount,
                    displayString = $"{completedCount}/{totalCount}",
                    toDoList = toDoList,
                    batches = dashboardBatches
                };

                return Result.Success(dashboardResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compiling dashboard statistics calculation summary block");
                return Result.Failure<ReadDashboardProgressDto>(ex.Message);
            }
        }
    }
}