using HRManagement.Domain.Interfaces;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace HRManagement.Application.Queries.Timesheet;

// ── Supervisor Dashboard ──────────────────────────────────────────────────────

/// <summary>
/// Returns the supervisor's dashboard data.
/// </summary>
public class GetSupervisorDashboardQuery : IRequest<ApiResponse<SupervisorDashboardResponseDto>>
{
    public int? FilterEmployeeId { get; init; }

    public class Handler(
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetSupervisorDashboardQuery, ApiResponse<SupervisorDashboardResponseDto>>
    {
        public async Task<ApiResponse<SupervisorDashboardResponseDto>> Handle(
            GetSupervisorDashboardQuery request,
            CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.AddHours(7);
            var supervisorEmail = currentUserService.Email;
            
            if (string.IsNullOrEmpty(supervisorEmail))
            {
                return ApiHelperResponse.Failed<SupervisorDashboardResponseDto>("Sesi Anda telah kedaluwarsa atau tidak valid. Silakan login kembali.");
            }

            var roles = await appDbContext.Roles.AsNoTracking().ToListAsync(cancellationToken);
            var supervisorRole = roles.FirstOrDefault(r => r.Name == "Supervisor");
            var employeeRole = roles.FirstOrDefault(r => r.Name == "Employee");
            
            if (supervisorRole == null || employeeRole == null)
            {
                return ApiHelperResponse.Failed<SupervisorDashboardResponseDto>("Roles configuration tidak valid. Harap hubungi administrator.");
            }

            var supervisor = await appDbContext.Employee.FirstOrDefaultAsync(e => e.EmployeeEmail == supervisorEmail, cancellationToken);
            if (supervisor == null)
            {
                return ApiHelperResponse.Failed<SupervisorDashboardResponseDto>("Akun Supervisor Anda tidak ditemukan di database. Harap hubungi administrator.");
            }

            if (supervisor.RoleId != supervisorRole.Id)
            {
                return ApiHelperResponse.Failed<SupervisorDashboardResponseDto>("Akses Ditolak. Halaman ini membutuhkan hak akses level Supervisor.");
            }

            var supervisorName = supervisor.FullName;

            // Filter interns (Relaxed to show all active interns for data visibility)
            var activeInterns = await appDbContext.Employee
                .AsNoTracking()
                .Where(e => e.RoleId == employeeRole.Id && e.IsActive)
                .ToListAsync(cancellationToken);
            
            var internIds = activeInterns.Select(i => i.Id).ToList();

            // Project summary - Optimized
            var totalProjects = await appDbContext.TimesheetProjects.CountAsync(p => !p.IsDeleted, cancellationToken);
            var runningProjects = await appDbContext.TimesheetProjects.CountAsync(p => p.Status == 0 && !p.IsDeleted, cancellationToken);
            var finishedProjects = totalProjects - runningProjects;

            // Pending approvals for MY interns (Daily Approval Logic)
            // If an intern has TimesheetEntries for a month, but that month is NOT fully approved (Status = 1), it needs approval.
            var entryMonths = await appDbContext.TimesheetEntries
                .AsNoTracking()
                .Where(e => internIds.Contains(e.EmployeeId) && !e.IsDeleted)
                .Select(e => new { e.EmployeeId, e.EntryDate.Year, e.EntryDate.Month })
                .Distinct()
                .ToListAsync(cancellationToken);

            var existingSubmissions = await appDbContext.TimesheetSubmissions
                .AsNoTracking()
                .Where(s => internIds.Contains(s.EmployeeId) && !s.IsDeleted)
                .ToListAsync(cancellationToken);

            var pendingDtos = new List<PendingApprovalItemDto>();
            foreach (var em in entryMonths)
            {
                var sub = existingSubmissions.FirstOrDefault(s => s.EmployeeId == em.EmployeeId && s.Year == em.Year && s.Month == em.Month);
                if (sub == null || sub.Status != 1)
                {
                    pendingDtos.Add(new PendingApprovalItemDto
                    {
                        SubmissionId = sub?.Id ?? 0,
                        EmployeeId = em.EmployeeId,
                        EmployeeName = activeInterns.FirstOrDefault(i => i.Id == em.EmployeeId)?.FullName ?? string.Empty,
                        Year = em.Year,
                        Month = em.Month,
                        SubmittedDate = sub?.SubmittedDate.ToString("yyyy-MM-dd HH:mm") ?? "",
                        Status = sub?.Status == 2 ? "Need Revision" : "Waiting for Approval"
                    });
                }
            }

            // 1. Missing Submissions (Current Month) - Optimized
            var submittedIds = await appDbContext.TimesheetSubmissions
                .AsNoTracking()
                .Where(s => s.Year == today.Year && s.Month == today.Month && internIds.Contains(s.EmployeeId) && !s.IsDeleted)
                .Select(s => s.EmployeeId)
                .ToListAsync(cancellationToken);

            var submittedSet = submittedIds.ToHashSet();

            var missingSubmissions = activeInterns
                .Where(i => !submittedSet.Contains(i.Id))
                .Select(i => new MissingSubmissionItemDto
                {
                    EmployeeId = i.Id,
                    EmployeeName = i.FullName,
                    Year = today.Year,
                    Month = today.Month,
                    OverdueDays = 0 
                }).ToList();

            // 2. Pending Approvals Summary
            var pendingCount = pendingDtos.Count;
            var totalPotentialSubmissions = activeInterns.Count;

            // 3. Intern hours breakdown (Optimized: Filter in SQL)
            var startOfMonth = new DateOnly(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var filteredEntries = await appDbContext.TimesheetEntries
                .AsNoTracking()
                .Where(e => internIds.Contains(e.EmployeeId) 
                    && e.EntryDate >= startOfMonth 
                    && e.EntryDate <= endOfMonth 
                    && !e.IsDeleted)
                .ToListAsync(cancellationToken);

            // Optional further filtering by specific intern if requested
            if (request.FilterEmployeeId.HasValue && internIds.Contains(request.FilterEmployeeId.Value))
            {
                filteredEntries = filteredEntries.Where(e => e.EmployeeId == request.FilterEmployeeId.Value).ToList();
            }

            var allProjects = await appDbContext.TimesheetProjects.AsNoTracking().ToListAsync(cancellationToken);
            foreach (var e in filteredEntries) {
                e.Employee = activeInterns.FirstOrDefault(i => i.Id == e.EmployeeId);
                e.Project = allProjects.FirstOrDefault(p => p.Id == e.ProjectId);
            }

            var internBreakdown = filteredEntries
                .GroupBy(e => new { e.EmployeeId, EmployeeName = e.Employee?.FullName ?? string.Empty })
                .Select(eg => new InternHoursBreakdownDto
                {
                    EmployeeId = eg.Key.EmployeeId,
                    EmployeeName = eg.Key.EmployeeName,
                    ProjectMinutes = eg
                        .GroupBy(e => e.Project?.Name ?? string.Empty)
                        .ToDictionary(pg => pg.Key, pg => pg.Sum(e => e.DurationMinutes)),
                    TotalMinutes = eg.Sum(e => e.DurationMinutes)
                })
                .OrderByDescending(x => x.TotalMinutes)
                .ToList();

            // 4. Project allocation donut chart data
            var grandTotal = filteredEntries.Sum(e => e.DurationMinutes);
            var projectAllocations = filteredEntries
                .GroupBy(e => new { e.ProjectId, ProjectName = e.Project?.Name ?? string.Empty })
                .Select(pg => new ProjectAllocationDto
                {
                    ProjectId = pg.Key.ProjectId,
                    ProjectName = pg.Key.ProjectName,
                    TotalMinutes = pg.Sum(e => e.DurationMinutes),
                    AllocationPercentage = grandTotal > 0
                        ? Math.Round((double)pg.Sum(e => e.DurationMinutes) / grandTotal * 100, 2)
                        : 0
                })
                .OrderByDescending(p => p.TotalMinutes)
                .ToList();




            // 5. Recent Activity (Live Feed)
            var recentEntries = await appDbContext.TimesheetEntries
                .AsNoTracking()
                .Where(e => internIds.Contains(e.EmployeeId) && !e.IsDeleted)
                .OrderByDescending(e => e.Id) // Assuming ID is incremental or use CreatedDate if available
                .Take(5)
                .ToListAsync(cancellationToken);

            foreach (var e in recentEntries) {
                e.Employee = activeInterns.FirstOrDefault(i => i.Id == e.EmployeeId);
                e.Project = allProjects.FirstOrDefault(p => p.Id == e.ProjectId);
            }

            var recentActivity = recentEntries.Select(e => new RecentActivityDto
            {
                EmployeeName = e.Employee?.FullName ?? "Unknown",
                ProjectName = e.Project?.Name ?? "General",
                DurationFormatted = $"{e.DurationMinutes / 60}h {e.DurationMinutes % 60}m",
                TaskDescription = e.TaskDescription,
                EntryDate = e.EntryDate.ToString("yyyy-MM-dd"),
                RelativeTime = "Just now" // Simplified for now
            }).ToList();

            // Final Result object
            var result = new SupervisorDashboardResponseDto
            {
                SupervisorName = supervisor?.FullName ?? "Supervisor",
                TotalActiveInterns = activeInterns.Count,
                TotalProjects = totalProjects,
                TotalRunningProjects = runningProjects,
                TotalFinishedProjects = finishedProjects,
                PendingApprovals = pendingDtos,
                MissingSubmissions = missingSubmissions,
                ApprovalSummaryCount = $"{pendingCount} / {totalPotentialSubmissions}",
                CurrentMonthLabel = today.ToString("MMM yyyy").ToUpper(),
                InternHoursBreakdown = internBreakdown,
                ProjectAllocations = projectAllocations,
                RecentActivity = recentActivity
            };

            return ApiHelperResponse.Success("Supervisor dashboard retrieved successfully.", result);
        }
    }
}




// ── Supervisor: Approval List (Report Menu) ───────────────────────────────────

/// <summary>
/// Returns all submission data grouped for the supervisor's report menu.
/// Includes: pending approvals, missing submissions, and approval history.
/// </summary>
public class GetApprovalReportQuery : IRequest<ApiResponse<SupervisorApprovalReportDto>>
{
    public class Handler(
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetApprovalReportQuery, ApiResponse<SupervisorApprovalReportDto>>
    {
        public async Task<ApiResponse<SupervisorApprovalReportDto>> Handle(
            GetApprovalReportQuery request,
            CancellationToken cancellationToken)
        {
            try 
            {
                var today = DateTime.UtcNow.AddHours(7);
                var currentYear = today.Year;
                var currentMonth = today.Month;

                var supervisorEmail = currentUserService.Email;

                if (string.IsNullOrEmpty(supervisorEmail))
                {
                    return ApiHelperResponse.Failed<SupervisorApprovalReportDto>("Sesi Anda telah kedaluwarsa. Silakan login kembali.");
                }

                var roles = await appDbContext.Roles.AsNoTracking().ToListAsync(cancellationToken);
                var supervisorRole = roles.FirstOrDefault(r => r.Name == "Supervisor");
                var employeeRole = roles.FirstOrDefault(r => r.Name == "Employee");
                
                if (supervisorRole == null || employeeRole == null)
                {
                    return ApiHelperResponse.Failed<SupervisorApprovalReportDto>("Roles configuration tidak valid. Harap hubungi administrator.");
                }

                var supervisor = await appDbContext.Employee.FirstOrDefaultAsync(e => e.EmployeeEmail == supervisorEmail, cancellationToken);
                if (supervisor == null)
                {
                    return ApiHelperResponse.Failed<SupervisorApprovalReportDto>("Akun Supervisor tidak ditemukan.");
                }

                // Internal Roles check
                if (supervisor.RoleId != supervisorRole.Id)
                {
                    return ApiHelperResponse.Failed<SupervisorApprovalReportDto>("Akses Ditolak. Halaman ini hanya untuk Supervisor.");
                }

                // All active interns
                var interns = await appDbContext.Employee
                    .AsNoTracking()
                    .Where(e => e.RoleId == employeeRole.Id && e.IsActive)
                    .ToListAsync(cancellationToken);
                
                var internIdList = interns.Select(i => i.Id).ToList();

                // All submissions for these interns
                var allSubmissions = await appDbContext.TimesheetSubmissions
                    .AsNoTracking()
                    .Where(s => internIdList.Contains(s.EmployeeId) && !s.IsDeleted)
                    .OrderByDescending(s => s.SubmittedDate)
                    .ToListAsync(cancellationToken);

                // MANUALLY HYDRATE Employee
                var submissionEmployeeIds = allSubmissions.Select(s => s.EmployeeId).Distinct().ToList();
                var submissionEmployees = await appDbContext.Employee
                    .Where(e => submissionEmployeeIds.Contains(e.Id))
                    .ToListAsync(cancellationToken);
                foreach (var s in allSubmissions) s.Employee = submissionEmployees.FirstOrDefault(e => e.Id == s.EmployeeId);

                // 1. Pending Approvals (status = 0)
                var pending = allSubmissions
                    .Where(s => s.Status == 0)
                    .Select(s => new PendingApprovalItemDto
                    {
                        SubmissionId = s.Id,
                        EmployeeId = s.EmployeeId,
                        EmployeeName = s.Employee?.FullName ?? string.Empty,
                        Year = s.Year,
                        Month = s.Month,
                        SubmittedDate = s.SubmittedDate.ToString("yyyy-MM-dd HH:mm"),
                        Status = "Waiting for Approval"
                    }).ToList();

                // 2. Missing Submissions
                var submittedEmployeeIdsCurrentMonth = allSubmissions
                    .Where(s => s.Year == currentYear && s.Month == currentMonth)
                    .Select(s => s.EmployeeId)
                    .ToHashSet();

                var start = new DateOnly(currentYear, currentMonth, 1);
                var end = (today.Year == currentYear && today.Month == currentMonth) 
                    ? DateOnly.FromDateTime(today) 
                    : start.AddMonths(1).AddDays(-1);
                
                var workingDays = new List<DateOnly>();
                for (var d = start; d <= end; d = d.AddDays(1)) 
                { 
                    if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday) workingDays.Add(d); 
                }

                var allEntriesCurrentMonth = await appDbContext.TimesheetEntries
                    .AsNoTracking()
                    .Where(e => internIdList.Contains(e.EmployeeId) && e.EntryDate >= start && e.EntryDate <= end && !e.IsDeleted)
                    .Select(e => new { e.EmployeeId, e.EntryDate })
                    .ToListAsync(cancellationToken);

                var entryMap = allEntriesCurrentMonth
                    .GroupBy(e => e.EmployeeId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.EntryDate).Distinct().ToHashSet());

                var missingSubmissions = new List<MissingSubmissionItemDto>();
                foreach (var intern in interns.Where(i => !submittedEmployeeIdsCurrentMonth.Contains(i.Id)))
                {
                    var internDates = entryMap.ContainsKey(intern.Id) ? entryMap[intern.Id] : new HashSet<DateOnly>();
                    var missingCount = workingDays.Count(wd => !internDates.Contains(wd));

                    missingSubmissions.Add(new MissingSubmissionItemDto
                    {
                        EmployeeId = intern.Id,
                        EmployeeName = intern.FullName,
                        Year = currentYear,
                        Month = currentMonth,
                        OverdueDays = missingCount
                    });
                }

                // 3. Approval History (status = 1 or 2)
                var history = allSubmissions
                    .Where(s => s.Status is 1 or 2)
                    .Select(s => new ApprovalHistoryItemDto
                    {
                        SubmissionId = s.Id,
                        EmployeeId = s.EmployeeId,
                        EmployeeName = s.Employee?.FullName ?? string.Empty,
                        Year = s.Year,
                        Month = s.Month,
                        SubmittedDate = s.SubmittedDate.ToString("yyyy-MM-dd HH:mm"),
                        Status = s.Status == 1 ? "Approved" : "Need Revision",
                        RevisionNote = s.RevisionNote,
                        ReviewedDate = s.ReviewedDate?.ToString("yyyy-MM-dd HH:mm")
                    }).ToList();

                var result = new SupervisorApprovalReportDto
                {
                    PendingApprovals = pending,
                    MissingSubmissions = missingSubmissions,
                    ApprovalHistory = history
                };

                return ApiHelperResponse.Success("Approval report retrieved successfully.", result);
            }
            catch (Exception ex)
            {
                return ApiHelperResponse.Failed<SupervisorApprovalReportDto>($"Internal Error: {ex.Message}");
            }
        }

    }
}

// ── Supervisor: Timesheet Review Page ────────────────────────────────────────

public class GetTimesheetReviewQuery : IRequest<ApiResponse<SupervisorReviewResponseDto>>
{
    public int SubmissionId { get; init; }
    public int? EmployeeId { get; init; }
    public int? Month { get; init; }
    public int? Year { get; init; }

    public GetTimesheetReviewQuery(int submissionId) { SubmissionId = submissionId; }
    public GetTimesheetReviewQuery(int employeeId, int month, int year) 
    { 
        EmployeeId = employeeId; 
        Month = month; 
        Year = year; 
    }

    public class Handler(
        ITimesheetEntryRepository entryRepository,
        ITimesheetSubmissionRepository submissionRepository,
        IApplicationDbContext appDbContext)
        : IRequestHandler<GetTimesheetReviewQuery, ApiResponse<SupervisorReviewResponseDto>>
    {
        public async Task<ApiResponse<SupervisorReviewResponseDto>> Handle(
            GetTimesheetReviewQuery request,
            CancellationToken cancellationToken)
        {
            int submissionId = 0;
            int employeeId = 0;
            string employeeName = string.Empty;
            int year = 0;
            int month = 0;
            int status = 0;
            string? revisionNote = null;
            DateTime? reviewedDate = null;

            if (request.SubmissionId > 0)
            {
                var s = await appDbContext.TimesheetSubmissions
                    .FirstOrDefaultAsync(s => s.Id == request.SubmissionId && !s.IsDeleted, cancellationToken);
                
                if (s != null) s.Employee = await appDbContext.Employee.FirstOrDefaultAsync(e => e.Id == s.EmployeeId, cancellationToken);
                
                if (s == null) return ApiHelperResponse.Failed<SupervisorReviewResponseDto>("Submission not found.");

                submissionId = s.Id;
                employeeId = s.EmployeeId;
                employeeName = s.Employee?.FullName ?? string.Empty;
                year = s.Year;
                month = s.Month;
                status = s.Status;
                revisionNote = s.RevisionNote;
                reviewedDate = s.ReviewedDate;
            }
            else if (request.EmployeeId.HasValue && request.Month.HasValue && request.Year.HasValue)
            {
                var s = await appDbContext.TimesheetSubmissions
                    .FirstOrDefaultAsync(s => s.EmployeeId == request.EmployeeId.Value 
                        && s.Month == request.Month.Value 
                        && s.Year == request.Year.Value 
                        && !s.IsDeleted, cancellationToken);
                
                if (s != null) s.Employee = await appDbContext.Employee.FirstOrDefaultAsync(e => e.Id == s.EmployeeId, cancellationToken);
                
                if (s != null)
                {
                    submissionId = s.Id;
                    employeeId = s.EmployeeId;
                    employeeName = s.Employee?.FullName ?? string.Empty;
                    year = s.Year;
                    month = s.Month;
                    status = s.Status;
                    revisionNote = s.RevisionNote;
                    reviewedDate = s.ReviewedDate;
                }
                else 
                {
                    var employee = await appDbContext.Employee.FindAsync(new object[] { request.EmployeeId.Value }, cancellationToken);
                    if (employee == null) return ApiHelperResponse.Failed<SupervisorReviewResponseDto>("Employee not found.");

                    submissionId = 0;
                    employeeId = request.EmployeeId.Value;
                    employeeName = employee.FullName;
                    year = request.Year.Value;
                    month = request.Month.Value;
                    status = 0; // Needs Approval (Default for virtual)
                }
            }

            if (employeeId == 0)
            {
                return ApiHelperResponse.Failed<SupervisorReviewResponseDto>("Review target not found.");
            }

            var entries = await entryRepository
                .GetEntriesByMonthAsync(employeeId, year, month);

            var comments = await submissionRepository.GetCommentsBySubmissionAsync(submissionId);

            // Build monthly day cells
            var days = entries
                .GroupBy(e => e.EntryDate)
                .Select(dg => new MonthlyDayCellDto
                {
                    Date = dg.Key.ToString("yyyy-MM-dd"),
                    TotalMinutes = dg.Sum(e => e.DurationMinutes),
                    ProjectMinutes = dg
                        .GroupBy(e => e.Project?.Name ?? string.Empty)
                        .ToDictionary(pg => pg.Key, pg => pg.Sum(e => e.DurationMinutes)),
                    Entries = dg.Select(e => new TimesheetEntryResponseDto
                    {
                        Id = e.Id,
                        DurationMinutes = e.DurationMinutes,
                        ProjectName = e.Project?.Name ?? string.Empty,
                        ApplicationUsed = e.ApplicationUsed,
                        TaskDescription = e.TaskDescription,
                        Location = e.Location switch
                        {
                            0 => "AIA Central",
                            1 => "WFH",
                            2 => "Meeting Room",
                            _ => "Unknown"
                        }
                    }).ToList()
                })
                .OrderBy(d => d.Date)
                .ToList();

            var dayCommentDtos = comments.Select(c => new DayCommentResponseDto
            {
                Date = c.CommentDate.ToString("yyyy-MM-dd"),
                Comment = c.Comment
            }).ToList();

            foreach (var d in days)
            {
                var match = dayCommentDtos.FirstOrDefault(c => c.Date == d.Date);
                if (match != null) d.Remark = match.Comment;
            }

            var result = new SupervisorReviewResponseDto
            {
                SubmissionId = submissionId,
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                Year = year,
                Month = month,
                Status = status switch
                {
                    0 => "Waiting for Approval",
                    1 => "Approved",
                    2 => "Need Revision",
                    _ => "Unknown"
                },
                RevisionNote = revisionNote,
                ReviewedDate = reviewedDate?.ToString("yyyy-MM-dd HH:mm"),
                DayComments = dayCommentDtos,
                Days = days
            };

            return ApiHelperResponse.Success("Timesheet review data retrieved successfully.", result);
        }
    }
}

// ── Projects ──────────────────────────────────────────────────────────────────

/// <summary>
/// Returns the list of all projects.
/// </summary>
public class GetProjectListQuery : IRequest<ApiResponse<List<ProjectDto>>>
{
    public class Handler(ITimesheetProjectRepository projectRepository)
        : IRequestHandler<GetProjectListQuery, ApiResponse<List<ProjectDto>>>
    {
        public async Task<ApiResponse<List<ProjectDto>>> Handle(
            GetProjectListQuery request,
            CancellationToken cancellationToken)
        {
            var projects = await projectRepository.GetActiveListAsync();
            var result = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ProjectLeader = p.ProjectLeader,
                Status = p.Status == 0 ? "Running" : "Finished"
            }).ToList();

            return ApiHelperResponse.Success("Projects retrieved successfully.", result);
        }
    }
}

// ── Supporting DTO ─────────────────────────────────────────────────────────────

/// <summary>Full supervisor approval report DTO.</summary>
public class SupervisorApprovalReportDto
{
    public List<PendingApprovalItemDto> PendingApprovals { get; set; } = new();
    public List<MissingSubmissionItemDto> MissingSubmissions { get; set; } = new();
    public List<ApprovalHistoryItemDto> ApprovalHistory { get; set; } = new();
}







