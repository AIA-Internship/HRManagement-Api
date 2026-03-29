using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries.Timesheet;

/// <summary>
/// Returns the dashboard data for the currently logged-in employee (intern).
/// </summary>
public class GetEmployeeDashboardQuery : IRequest<ApiResponse<DashboardResponseDto>>
{
    public class Handler(
        ITimesheetRepository timesheetRepository,
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetEmployeeDashboardQuery, ApiResponse<DashboardResponseDto>>
    {
        public async Task<ApiResponse<DashboardResponseDto>> Handle(
            GetEmployeeDashboardQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var today = DateTime.UtcNow.AddHours(7);
            var year = today.Year;
            var month = today.Month;

            // 1. Sequentially fetch data to avoid DbContext concurrency issues
            var employee = await appDbContext.Employees.FindAsync(new object[] { employeeId }, cancellationToken);
            var employmentInfo = await appDbContext.EmploymentInformation.FirstOrDefaultAsync(ei => ei.EmployeeId == employeeId, cancellationToken);
            var submission = await timesheetRepository.GetSubmissionAsync(employeeId, year, month);
            var monthlyEntries = await timesheetRepository.GetEntriesByMonthAsync(employeeId, year, month);
            var missingDates = await timesheetRepository.GetMissingEntryDatesAsync(employeeId, year, month);
            var todoTasks = await timesheetRepository.GetTodoTasksByEmployeeAsync(employeeId);

            // 2. Days until end of month (submission deadline)
            var lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var daysUntilDeadline = (lastDay.Date - today.Date).Days;

            // 4. Map results
            var submissionStatus = MapSubmissionStatus(submission, year, month, daysUntilDeadline);

            var projectSummary = monthlyEntries
                .GroupBy(e => new { e.ProjectId, e.Project.Name })
                .Select(g => new ProjectSummaryDto
                {
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.Name,
                    TotalLoggedMinutes = g.Sum(e => e.DurationMinutes),
                    TotalLoggedFormatted = FormatMinutes(g.Sum(e => e.DurationMinutes))
                })
                .ToList();

            var totalLoggedAll = projectSummary.Sum(p => p.TotalLoggedMinutes);
            var projectAllocations = projectSummary.Select(p => new ProjectAllocationDto
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName,
                TotalMinutes = p.TotalLoggedMinutes,
                AllocationPercentage = totalLoggedAll > 0 ? Math.Round((double)p.TotalLoggedMinutes / totalLoggedAll * 100, 1) : 0
            }).ToList();

            var missingDays = missingDates
                .Select(d => new MissingDayDto
                {
                    Date = d.ToString("yyyy-MM-dd"),
                    DayOfWeek = d.DayOfWeek.ToString()
                })
                .ToList();

            var todoList = todoTasks.Select(t => new TodoTaskResponseDto
            {
                Id = t.Id,
                TaskName = t.TaskName,
                DueDate = t.DueDate?.ToString("yyyy-MM-dd"),
                Priority = MapPriority(t.Priority),
                IsCompleted = t.IsCompleted
            }).ToList();

            var response = new DashboardResponseDto
            {
                EmployeeName = employee?.FullName ?? "Employee",
                SupervisorName = employmentInfo?.SupervisorName ?? "---",
                DaysUntilDeadline = daysUntilDeadline,
                CurrentMonthSubmission = submissionStatus,
                ProjectAllocations = projectAllocations,
                AssignedProjects = projectSummary,
                MissingDays = missingDays,
                TodoTasks = todoList
            };

            return ApiHelperResponse.Success("Dashboard data retrieved successfully.", response);
        }

        private static SubmissionStatusDto MapSubmissionStatus(
            HRManagement.Api.Domain.Models.Tables.TimesheetSubmission? sub, int year, int month, int daysRemaining)
        {
            var monthName = new DateTime(year, month, 1).ToString("MMMM");
            if (sub == null)
            {
                return new SubmissionStatusDto
                {
                    Year = year,
                    Month = month,
                    MonthName = monthName,
                    DaysRemaining = daysRemaining,
                    Status = "Not Submitted"
                };
            }

            return new SubmissionStatusDto
            {
                SubmissionId = sub.Id,
                Year = sub.Year,
                Month = sub.Month,
                MonthName = monthName,
                DaysRemaining = daysRemaining,
                Status = MapStatus(sub.Status),
                SubmittedDate = sub.SubmittedDate.ToString("yyyy-MM-dd HH:mm"),
                ReviewedDate = sub.ReviewedDate?.ToString("yyyy-MM-dd HH:mm"),
                RevisionNote = sub.RevisionNote
            };
        }

        private static string MapStatus(int status) => status switch
        {
            0 => "Waiting for Approval",
            1 => "Approved",
            2 => "Need Revision",
            _ => "Unknown"
        };

        private static string MapPriority(int priority) => priority switch
        {
            0 => "Low",
            1 => "Medium",
            2 => "High",
            _ => "Low"
        };

        private static string FormatMinutes(int minutes)
        {
            var h = minutes / 60;
            var m = minutes % 60;
            return h > 0 ? $"{h}h {m}m" : $"{m}m";
        }
    }
}
