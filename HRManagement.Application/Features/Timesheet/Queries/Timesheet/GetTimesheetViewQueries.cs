using HRManagement.Domain.Interfaces;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Application.Queries.Timesheet;

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
        ITimesheetSubmissionRepository submissionRepository,
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

            // Fetch submission to get comments
            var submission = await submissionRepository.GetSubmissionAsync(employeeId, entryDate.Year, entryDate.Month);
            var supervisorRemark = string.Empty;
            if (submission != null)
            {
                var comments = await submissionRepository.GetCommentsBySubmissionAsync(submission.Id);
                var dayComment = comments.FirstOrDefault(c => c.CommentDate == entryDate);
                if (dayComment != null)
                {
                    supervisorRemark = dayComment.Comment;
                }
            }

            var submissionStatusStr = submission == null ? "Not Submitted" : submission.Status switch
            {
                0 => "Needs Approval",
                1 => "Approved",
                2 => "Need Revision",
                _ => "Not Submitted"
            };

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
                ProjectLeadName = e.Project != null && !string.IsNullOrEmpty(e.Project.ProjectLeader) ? e.Project.ProjectLeader : (e.ProjectLead?.FullName ?? string.Empty),
                Location = MapLocation(e.Location)
            }).ToList();

            var totalMinutes = entryDtos.Sum(e => e.DurationMinutes);
            
            string dayStatus = "Not Submitted";
            if (totalMinutes > 0)
            {
                if (submissionStatusStr == "Needs Approval" || submissionStatusStr == "Not Submitted") {
                    dayStatus = "Needs Approval";
                } else if (submissionStatusStr == "Approved") {
                    dayStatus = "Approved";
                } else if (submissionStatusStr == "Need Revision") {
                    if (!string.IsNullOrEmpty(supervisorRemark) && supervisorRemark != "[APPROVED]") {
                        dayStatus = "Needs Revision";
                    } else {
                        dayStatus = "Approved";
                    }
                }
            }

            var result = new DailyTimesheetResponseDto
            {
                Date = entryDate.ToString("yyyy-MM-dd"),
                TotalMinutes = totalMinutes,
                TotalFormatted = FormatMinutes(totalMinutes),
                SupervisorRemark = supervisorRemark,
                SubmissionStatus = dayStatus,
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
        ITimesheetSubmissionRepository submissionRepository,
        IApplicationDbContext appDbContext,
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

            // Fetch submission for the month (assume week falls primarily in one month, use weekStart)
            var submission = await submissionRepository.GetSubmissionAsync(employeeId, weekStart.Year, weekStart.Month);
            var comments = new List<HRManagement.Domain.Models.Tables.TimesheetDayComment>();
            if (submission != null)
            {
                comments = await appDbContext.TimesheetDayComments
                    .AsNoTracking()
                    .Where(c => c.SubmissionId == submission.Id && !c.IsDeleted)
                    .ToListAsync(cancellationToken);
            }

            var submissionStatusStr = submission == null ? "Not Submitted" : submission.Status switch
            {
                0 => "Needs Approval",
                1 => "Approved",
                2 => "Need Revision",
                _ => "Not Submitted"
            };

            // Group by Date to match the new Weekly UI requirement
            var dayRows = new List<WeeklyDayRowDto>();

            for (int i = 0; i < 7; i++)
            {
                var currentDate = weekStart.AddDays(i);
                var dailyEntries = entries.Where(e => e.EntryDate == currentDate).ToList();
                
                var totalMinutes = dailyEntries.Sum(e => e.DurationMinutes);
                
                // Construct lists, skipping empty values
                var projects = dailyEntries.Where(e => e.Project != null).Select(e => e.Project!.Name).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var apps = dailyEntries.Select(e => e.ApplicationUsed).Where(x => !string.IsNullOrEmpty(x) && x != "-").Distinct().ToList();
                var tasks = dailyEntries.Select(e => e.TaskDescription).Where(x => !string.IsNullOrEmpty(x) && x != "-").Distinct().ToList();
                var locations = dailyEntries.Select(e => MapLocation(e.Location)).Distinct().ToList();
                
                // Determine remark
                string remark = string.Empty;
                
                // Find if there is an explicit DayType mapping to holiday or leave
                var explicitRemark = dailyEntries.FirstOrDefault(e => e.DayType == "holiday" || e.DayType == "leave" || e.DayType == "off");
                if (explicitRemark != null) {
                    remark = explicitRemark.DayType == "holiday" ? "HOLIDAY" : 
                             explicitRemark.DayType == "leave" ? "PERSONAL LEAVE" : "OFF";
                }

                var dayComment = comments.FirstOrDefault(c => c.CommentDate == currentDate)?.Comment;
                
                string dayStatus = "";
                if (totalMinutes > 0)
                {
                    if (submissionStatusStr == "Needs Approval" || submissionStatusStr == "Not Submitted") {
                        dayStatus = "Needs Approval";
                    } else if (submissionStatusStr == "Approved") {
                        dayStatus = "Approved";
                    } else if (submissionStatusStr == "Need Revision") {
                        if (!string.IsNullOrEmpty(dayComment) && dayComment != "[APPROVED]") {
                            dayStatus = "Needs Revision";
                        } else {
                            dayStatus = "Approved";
                        }
                    }
                }

                dayRows.Add(new WeeklyDayRowDto
                {
                    Date = currentDate.ToString("d-MMM"), // e.g. 2-Feb
                    DayOfWeek = currentDate.DayOfWeek.ToString(),
                    Status = dayStatus,
                    TotalMinutes = totalMinutes,
                    TotalFormatted = FormatMinutes(totalMinutes),
                    Projects = projects,
                    AppsUsed = apps,
                    Tasks = tasks,
                    Locations = locations,
                    Remark = remark,
                    HasComment = !string.IsNullOrEmpty(dayComment) && dayComment != "[APPROVED]"
                });
            }

            var grandTotal = entries.Sum(e => e.DurationMinutes);
            var result = new WeeklyTimesheetResponseDto
            {
                WeekStart = weekStart.ToString("yyyy-MM-dd"),
                WeekEnd = weekEnd.ToString("yyyy-MM-dd"),
                GrandTotalMinutes = grandTotal,
                GrandTotalFormatted = FormatMinutes(grandTotal),
                SubmissionStatus = submissionStatusStr,
                Days = dayRows
            };

            return ApiHelperResponse.Success("Weekly timesheet retrieved successfully.", result);
        }

        private static string MapLocation(int loc) => loc switch
        {
            0 => "AIA Central",
            1 => "WFH",
            2 => "Meeting Room",
            _ => "AIA Central"
        };

        private static string FormatMinutes(int minutes)
        {
            var h = minutes / 60;
            var m = minutes % 60;
            return h > 0 ? $"{h}.{m / 6} h" : "0.0 h"; // Formatting as 8.0 h
        }
    }
}

// ── Report View ───────────────────────────────────────────────────────────────

/// <summary>
/// Returns detailed monthly timesheet view for the Report page.
/// </summary>
public class GetReportTimesheetQuery(int year, int month, int? targetEmployeeId = null)
    : IRequest<ApiResponse<ReportTimesheetResponseDto>>
{
    public int Year { get; } = year;
    public int Month { get; } = month;
    public int? TargetEmployeeId { get; } = targetEmployeeId;

    public class Handler(
        ITimesheetEntryRepository entryRepository,
        ITimesheetSubmissionRepository submissionRepository,
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetReportTimesheetQuery, ApiResponse<ReportTimesheetResponseDto>>
    {
        public async Task<ApiResponse<ReportTimesheetResponseDto>> Handle(
            GetReportTimesheetQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = request.TargetEmployeeId ?? currentUserService.UserId;
            
            // Get Employee info
            var employee = await appDbContext.Employee
                .Include(e => e.EmploymentInformation)
                .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
            
            if (employee == null) return ApiHelperResponse.Failed<ReportTimesheetResponseDto>("Employee not found");

            var entries = await entryRepository.GetEntriesByMonthAsync(employeeId, request.Year, request.Month);
            var submission = await submissionRepository.GetSubmissionAsync(employeeId, request.Year, request.Month);

            var submissionStatusStr = submission == null ? "Not Submitted" : submission.Status switch
            {
                0 => "Needs Approval",
                1 => "Approved",
                2 => "Need Revision",
                _ => "Not Submitted"
            };

            var dayRows = new List<WeeklyDayRowDto>();
            int daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);

            for (int i = 1; i <= daysInMonth; i++)
            {
                var currentDate = new DateOnly(request.Year, request.Month, i);
                var dailyEntries = entries.Where(e => e.EntryDate == currentDate).ToList();
                
                var totalMinutes = dailyEntries.Sum(e => e.DurationMinutes);
                
                var projects = dailyEntries.Where(e => e.Project != null).Select(e => e.Project!.Name).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var apps = dailyEntries.Select(e => e.ApplicationUsed).Where(x => !string.IsNullOrEmpty(x) && x != "-").Distinct().ToList();
                var tasks = dailyEntries.Select(e => e.TaskDescription).Where(x => !string.IsNullOrEmpty(x) && x != "-").Distinct().ToList();
                var locations = dailyEntries.Select(e => MapLocation(e.Location)).Distinct().ToList();
                
                string remark = string.Empty;
                
                var explicitRemark = dailyEntries.FirstOrDefault(e => e.DayType == "holiday" || e.DayType == "leave" || e.DayType == "off");
                if (explicitRemark != null) {
                    remark = explicitRemark.DayType == "holiday" ? "HOLIDAY" : 
                             explicitRemark.DayType == "leave" ? "LEAVE" : "OFF";
                }

                // If it's empty and not weekend/holiday, skip rendering if requested? 
                // The prompt says "yang muncul yang udah diisi semuanya baik yang udah diapprove or belum"
                // But the screenshot shows weekends with "0.0 h" and "OFF". 
                // It's easier to return all days. The frontend can filter if needed, or we just render all days as they appear in the mockup.
                // The mockup shows all days up to 13-Feb. Let's return all days up to today if it's the current month, or all days if past month.
                
                var isFuture = currentDate > DateOnly.FromDateTime(DateTime.UtcNow.Date);
                if (isFuture && totalMinutes == 0 && remark == string.Empty) continue; // Skip future empty days
                
                // For past days, if it's completely empty and not a weekend/holiday, maybe still show it with 0.0h.
                if (totalMinutes == 0 && string.IsNullOrEmpty(remark)) continue;

                dayRows.Add(new WeeklyDayRowDto
                {
                    Date = currentDate.ToString("d-MMM"), // e.g. 1-Feb
                    DayOfWeek = currentDate.DayOfWeek.ToString(),
                    TotalMinutes = totalMinutes,
                    TotalFormatted = FormatMinutes(totalMinutes),
                    Projects = projects,
                    AppsUsed = apps,
                    Tasks = tasks,
                    Locations = locations,
                    Remark = remark
                });
            }

            var grandTotal = entries.Sum(e => e.DurationMinutes);
            var result = new ReportTimesheetResponseDto
            {
                Year = request.Year,
                Month = request.Month,
                MonthName = new DateTime(request.Year, request.Month, 1).ToString("MMMM"),
                SupervisorName = employee.EmploymentInformation?.SupervisorName ?? "-",
                EmployeeName = employee.FullName,
                GrandTotalMinutes = grandTotal,
                GrandTotalFormatted = FormatMinutes(grandTotal),
                SubmissionStatus = submissionStatusStr,
                Days = dayRows
            };

            return ApiHelperResponse.Success("Report timesheet retrieved successfully.", result);
        }

        private static string MapLocation(int loc) => loc switch
        {
            0 => "AIA Central",
            1 => "WFH",
            2 => "Meeting Room",
            _ => "AIA Central"
        };

        private static string FormatMinutes(int minutes)
        {
            var h = minutes / 60;
            var m = minutes % 60;
            return h > 0 ? $"{h}.{m / 6} h" : "0.0 h"; // Formatting as 8.0 h
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
        ICurrentUserService currentUserService,
        IApplicationDbContext appDbContext)
        : IRequestHandler<GetMonthlyTimesheetQuery, ApiResponse<MonthlyTimesheetResponseDto>>
    {
        public async Task<ApiResponse<MonthlyTimesheetResponseDto>> Handle(
            GetMonthlyTimesheetQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = request.TargetEmployeeId ?? currentUserService.UserId;
            var entries = await entryRepository.GetEntriesByMonthAsync(employeeId, request.Year, request.Month);
            var submission = await submissionRepository.GetSubmissionAsync(employeeId, request.Year, request.Month);

            // Get remarks for this submission
            var comments = new List<HRManagement.Domain.Models.Tables.TimesheetDayComment>();
            if (submission != null)
            {
                comments = await appDbContext.TimesheetDayComments
                    .AsNoTracking()
                    .Where(c => c.SubmissionId == submission.Id && !c.IsDeleted)
                    .ToListAsync(cancellationToken);
            }

            // Build day cells
            var grouped = entries
                .GroupBy(e => e.EntryDate)
                .Select(dg => {
                    var remark = "";
                    var explicitRemark = dg.FirstOrDefault(e => e.DayType == "holiday" || e.DayType == "leave" || e.DayType == "off");
                    if (explicitRemark != null) {
                        remark = explicitRemark.DayType == "holiday" ? "HOLIDAY" : 
                                 explicitRemark.DayType == "leave" ? "PERSONAL LEAVE" : "OFF";
                    }
                    var cellDate = dg.Key;
                    var dayComment = comments.FirstOrDefault(c => c.CommentDate == cellDate)?.Comment ?? "";

                    return new MonthlyDayCellDto
                    {
                        Date = dg.Key.ToString("yyyy-MM-dd"),
                        TotalMinutes = dg.Sum(e => e.DurationMinutes),
                        ProjectMinutes = dg
                            .GroupBy(e => e.Project?.Name ?? string.Empty)
                            .ToDictionary(pg => pg.Key, pg => pg.Sum(e => e.DurationMinutes)),
                        Remark = remark,
                        SupervisorRemark = dayComment
                    };
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




