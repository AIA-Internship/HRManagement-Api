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

        public GetInternDashboardProgressHandler(IELearningRepository repo, ILogger<GetInternDashboardProgressHandler> logger)
        {
            _repo = repo;
            _logger = logger;
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

                var dashboardResult = new ReadDashboardProgressDto
                {
                    totalModules = totalCount,
                    completedModules = completedCount,
                    displayString = $"{completedCount}/{totalCount}",
                    toDoList = toDoList
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