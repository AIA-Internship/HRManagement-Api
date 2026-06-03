using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Queries.Timesheet;

// ── Daily View ────────────────────────────────────────────────────────────────

/// <summary>
/// Returns all timesheet entries for a specific date.
/// Can be called by both employee (own data) and supervisor (any employee's data).
/// </summary>
public class GetDailyTimesheetQuery(string date, int? targetEmployeeId = null)
    : IRequest<ApiResponse<DailyTimesheetResponseDto>>
{
    public string Date { get; } = date;
    public int? TargetEmployeeId { get; } = targetEmployeeId;

    public class Handler(
        ITimesheetEntryRepository entryRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetDailyTimesheetQuery, ApiResponse<DailyTimesheetResponseDto>>
    {
        public async Task<ApiResponse<DailyTimesheetResponseDto>> Handle(
            GetDailyTimesheetQuery request,
            CancellationToken cancellationToken)
        {
            if (!DateOnly.TryParseExact(request.Date, "yyyy-MM-dd", out var entryDate))
            {
                return ApiHelperResponse.Failed<DailyTimesheetResponseDto>("Invalid date format. Expected yyyy-MM-dd.");
            }

            var employeeId = request.TargetEmployeeId ?? currentUserService.UserId;
            var entries = await entryRepository.GetEntriesByDateAsync(employeeId, entryDate);

            var entryDtos = entries.Select(e => new TimesheetEntryResponseDto
            {
                Id = e.Id,
                Date = e.EntryDate.ToString("yyyy-MM-dd"),
                DurationMinutes = e.DurationMinutes,
                DurationFormatted = FormatMinutes(e.DurationMinutes),
                ProjectId = e.ProjectId,
                ProjectName = e.Project?.Name ?? string.Empty,
                ApplicationUsed = e.ApplicationUsed,
                TaskDescription = e.TaskDescription,
                ProjectLeadId = e.ProjectLeadId,
                ProjectLeadName = e.ProjectLead?.FullName ?? string.Empty,
                Location = MapLocation(e.Location)
            }).ToList();

            var totalMinutes = entryDtos.Sum(e => e.DurationMinutes);
            var result = new DailyTimesheetResponseDto
            {
                Date = entryDate.ToString("yyyy-MM-dd"),
                TotalMinutes = totalMinutes,
                TotalFormatted = FormatMinutes(totalMinutes),
                Entries = entryDtos
            };

            return ApiHelperResponse.Success("Daily timesheet retrieved successfully.", result);
        }

        private static string MapLocation(int loc) => loc switch
        {
            0 => "Office",
            1 => "WFH",
            2 => "Meeting Room",
            _ => "Office"
        };

        private static string FormatMinutes(int minutes)
        {
            var h = minutes / 60;
            var m = minutes % 60;
            return h > 0 ? $"{h}h {m}m" : $"{m}m";
        }
    }
}

// ── Weekly View ───────────────────────────────────────────────────────────────

/// <summary>
/// Returns weekly timesheet view aggregated by project per day.
/// </summary>
public class GetWeeklyTimesheetQuery(string weekStartDate, int? targetEmployeeId = null)
    : IRequest<ApiResponse<WeeklyTimesheetResponseDto>>
{
    public string WeekStartDate { get; } = weekStartDate;
    public int? TargetEmployeeId { get; } = targetEmployeeId;

    public class Handler(
        ITimesheetEntryRepository entryRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetWeeklyTimesheetQuery, ApiResponse<WeeklyTimesheetResponseDto>>
    {
        public async Task<ApiResponse<WeeklyTimesheetResponseDto>> Handle(
            GetWeeklyTimesheetQuery request,
            CancellationToken cancellationToken)
        {
            if (!DateOnly.TryParseExact(request.WeekStartDate, "yyyy-MM-dd", out var weekStart))
            {
                return ApiHelperResponse.Failed<WeeklyTimesheetResponseDto>("Invalid date format. Expected yyyy-MM-dd.");
            }

            var weekEnd = weekStart.AddDays(6);
            var employeeId = request.TargetEmployeeId ?? currentUserService.UserId;
            var entries = await entryRepository.GetEntriesByWeekAsync(employeeId, weekStart, weekEnd);

            // Group by project, then by date
            var projectGroups = entries
                .GroupBy(e => new { e.ProjectId, ProjectName = e.Project?.Name ?? string.Empty })
                .Select(pg =>
                {
                    var dailyMinutes = pg
                        .GroupBy(e => e.EntryDate.ToString("yyyy-MM-dd"))
                        .ToDictionary(dg => dg.Key, dg => dg.Sum(e => e.DurationMinutes));

                    var weeklyTotal = pg.Sum(e => e.DurationMinutes);
                    return new WeeklyProjectRowDto
                    {
                        ProjectId = pg.Key.ProjectId,
                        ProjectName = pg.Key.ProjectName,
                        DailyMinutes = dailyMinutes,
                        WeeklyTotalMinutes = weeklyTotal,
                        WeeklyTotalFormatted = FormatMinutes(weeklyTotal)
                    };
                })
                .ToList();

            var grandTotal = entries.Sum(e => e.DurationMinutes);
            var result = new WeeklyTimesheetResponseDto
            {
                WeekStart = weekStart.ToString("yyyy-MM-dd"),
                WeekEnd = weekEnd.ToString("yyyy-MM-dd"),
                GrandTotalMinutes = grandTotal,
                GrandTotalFormatted = FormatMinutes(grandTotal),
                Projects = projectGroups
            };

            return ApiHelperResponse.Success("Weekly timesheet retrieved successfully.", result);
        }

        private static string FormatMinutes(int minutes)
        {
            var h = minutes / 60;
            var m = minutes % 60;
            return h > 0 ? $"{h}h {m}m" : $"{m}m";
        }
    }
}

// ── Monthly View ──────────────────────────────────────────────────────────────

/// <summary>
/// Returns monthly timesheet view with per-day cell data.
/// </summary>
public class GetMonthlyTimesheetQuery(int year, int month, int? targetEmployeeId = null)
    : IRequest<ApiResponse<MonthlyTimesheetResponseDto>>
{
    public int Year { get; } = year;
    public int Month { get; } = month;
    public int? TargetEmployeeId { get; } = targetEmployeeId;

    public class Handler(
        ITimesheetEntryRepository entryRepository,
        ITimesheetSubmissionRepository submissionRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetMonthlyTimesheetQuery, ApiResponse<MonthlyTimesheetResponseDto>>
    {
        public async Task<ApiResponse<MonthlyTimesheetResponseDto>> Handle(
            GetMonthlyTimesheetQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = request.TargetEmployeeId ?? currentUserService.UserId;
            var entries = await entryRepository.GetEntriesByMonthAsync(employeeId, request.Year, request.Month);
            var submission = await submissionRepository.GetSubmissionAsync(employeeId, request.Year, request.Month);

            // Build day cells
            var grouped = entries
                .GroupBy(e => e.EntryDate)
                .Select(dg => new MonthlyDayCellDto
                {
                    Date = dg.Key.ToString("yyyy-MM-dd"),
                    TotalMinutes = dg.Sum(e => e.DurationMinutes),
                    ProjectMinutes = dg
                        .GroupBy(e => e.Project?.Name ?? string.Empty)
                        .ToDictionary(pg => pg.Key, pg => pg.Sum(e => e.DurationMinutes))
                })
                .ToList();

            var submissionStatus = submission == null ? "Not Submitted" : submission.Status switch
            {
                0 => "Waiting for Approval",
                1 => "Approved",
                2 => "Need Revision",
                _ => "Not Submitted"
            };

            var result = new MonthlyTimesheetResponseDto
            {
                Year = request.Year,
                Month = request.Month,
                SubmissionId = submission?.Id,
                SubmissionStatus = submissionStatus,
                Days = grouped
            };

            return ApiHelperResponse.Success("Monthly timesheet retrieved successfully.", result);

        }
    }
}
