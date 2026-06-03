using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HRManagement.Api.Domain.Models.Tables.MasterRole;

namespace HRManagement.Api.Application.Queries.Timesheet;

// ── Supervisor Dashboard ──────────────────────────────────────────────────────

/// <summary>
/// Returns the supervisor's dashboard data.
/// </summary>
public class GetSupervisorDashboardQuery : IRequest<ApiResponse<SupervisorDashboardResponseDto>>
{
    public int? FilterEmployeeId { get; init; }

    public class Handler(
        ITimesheetRepository timesheetRepository,
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
                return ApiHelperResponse.Failed<SupervisorDashboardResponseDto>("Role configuration tidak valid. Harap hubungi administrator.");
            }

            var supervisor = await appDbContext.Employees.FirstOrDefaultAsync(e => e.EmployeeEmail == supervisorEmail, cancellationToken);
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
            var activeInterns = await appDbContext.Employees
                .AsNoTracking()
                .Where(e => e.RoleId == employeeRole.Id && e.IsActive)
                .ToListAsync(cancellationToken);
            
            var internIds = activeInterns.Select(i => i.Id).ToList();

            // Project summary
            var allProjects = await timesheetRepository.GetAllProjectsAsync();
            var totalProjects = allProjects.Count;
            var runningProjects = allProjects.Count(p => p.Status == 0);
            var finishedProjects = allProjects.Count(p => p.Status == 1);

            // Pending approvals for MY interns
            var pending = await timesheetRepository.GetPendingSubmissionsAsync();
            pending = pending.Where(s => internIds.Contains(s.EmployeeId)).ToList();
            var pendingDtos = pending.Select(s => new PendingApprovalItemDto
            {
                SubmissionId = s.Id,
                EmployeeId = s.EmployeeId,
                EmployeeName = s.Employee?.FullName ?? string.Empty,
                Year = s.Year,
                Month = s.Month,
                SubmittedDate = s.SubmittedDate.ToString("yyyy-MM-dd HH:mm"),
                Status = "Waiting for Approval"
            }).ToList();

            // 1. Missing Submissions (Current Month) — interns who haven't submitted
            var submittedEmployeeIds = pending.Select(s => s.EmployeeId).ToHashSet();
            // Also check history for this month
            var history = await timesheetRepository.GetAllSubmissionsAsync();
            var monthHistoryIds = history
                .Where(s => s.Year == today.Year && s.Month == today.Month)
                .Select(s => s.EmployeeId);
            
            foreach(var id in monthHistoryIds) submittedEmployeeIds.Add(id);

            var missingSubmissions = activeInterns
                .Where(i => !submittedEmployeeIds.Contains(i.Id))
                .Select(i => new MissingSubmissionItemDto
                {
                    EmployeeId = i.Id,
                    EmployeeName = i.FullName,
                    Year = today.Year,
                    Month = today.Month,
                    OverdueDays = 0 // Placeholder
                }).ToList();

            // 2. Pending Approvals Summary
            var pendingCount = pendingDtos.Count;
            var totalPotentialSubmissions = activeInterns.Count; // Simplified logic

            // 3. Intern hours breakdown for current month (ONLY for my interns)
            var allMonthEntries = await timesheetRepository
                .GetAllEntriesByMonthForAllEmployeesAsync(today.Year, today.Month);

            allMonthEntries = allMonthEntries.Where(e => internIds.Contains(e.EmployeeId)).ToList();

            var filteredEntries = request.FilterEmployeeId.HasValue && internIds.Contains(request.FilterEmployeeId.Value)
                ? allMonthEntries.Where(e => e.EmployeeId == request.FilterEmployeeId.Value).ToList()
                : allMonthEntries;

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
            var grandTotal = allMonthEntries.Sum(e => e.DurationMinutes);
            var projectAllocations = allMonthEntries
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
                ProjectAllocations = projectAllocations
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
        ITimesheetRepository timesheetRepository,
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetApprovalReportQuery, ApiResponse<SupervisorApprovalReportDto>>
    {
        public async Task<ApiResponse<SupervisorApprovalReportDto>> Handle(
            GetApprovalReportQuery request,
            CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.AddHours(7);
            var currentYear = today.Year;
            var currentMonth = today.Month;

            var supervisorEmail = currentUserService.Email;

            if (string.IsNullOrEmpty(supervisorEmail))
            {
                return ApiHelperResponse.Failed<SupervisorApprovalReportDto>("Sesi Anda telah kedaluwarsa atau tidak valid. Silakan login kembali.");
            }

            var roles = await appDbContext.Roles.AsNoTracking().ToListAsync(cancellationToken);
            var supervisorRole = roles.FirstOrDefault(r => r.Name == "Supervisor");
            var employeeRole = roles.FirstOrDefault(r => r.Name == "Employee");
            
            if (supervisorRole == null || employeeRole == null)
            {
                return ApiHelperResponse.Failed<SupervisorApprovalReportDto>("Role configuration tidak valid. Harap hubungi administrator.");
            }

            var supervisor = await appDbContext.Employees.FirstOrDefaultAsync(e => e.EmployeeEmail == supervisorEmail, cancellationToken);
            if (supervisor == null)
            {
                return ApiHelperResponse.Failed<SupervisorApprovalReportDto>("Akun Supervisor Anda tidak ditemukan di database. Harap hubungi administrator.");
            }

            if (supervisor.RoleId != supervisorRole.Id)
            {
                return ApiHelperResponse.Failed<SupervisorApprovalReportDto>("Akses Ditolak. Anda tidak memiliki wewenang untuk melihat laporan izin ini.");
            }

            var supervisorName = supervisor.FullName;

            // All active interns
            var interns = await appDbContext.Employees
                .AsNoTracking()
                .Where(e => e.RoleId == employeeRole.Id && e.IsActive)
                .ToListAsync(cancellationToken);
            
            var internIds = interns.Select(i => i.Id).ToHashSet();

            // All submissions for MY interns
            var allSubmissions = await timesheetRepository.GetAllSubmissionsAsync();
            allSubmissions = allSubmissions.Where(s => internIds.Contains(s.EmployeeId)).ToList();

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

            // 2. Missing Submissions — interns who haven't submitted for the current month
            var submittedEmployeeIds = allSubmissions
                .Where(s => s.Year == currentYear && s.Month == currentMonth)
                .Select(s => s.EmployeeId)
                .ToHashSet();

            var missingSubmissions = new List<MissingSubmissionItemDto>();
            foreach (var intern in interns.Where(i => !submittedEmployeeIds.Contains(i.Id)))
            {
                var missingDays = await timesheetRepository
                    .GetMissingEntryDatesAsync(intern.Id, currentYear, currentMonth);

                missingSubmissions.Add(new MissingSubmissionItemDto
                {
                    EmployeeId = intern.Id,
                    EmployeeName = intern.FullName,
                    Year = currentYear,
                    Month = currentMonth,
                    OverdueDays = missingDays.Count
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
    }
}

// ── Supervisor: Timesheet Review Page ────────────────────────────────────────

/// <summary>
/// Returns the full review page data for a specific submission.
/// </summary>
public class GetTimesheetReviewQuery(int submissionId)
    : IRequest<ApiResponse<SupervisorReviewResponseDto>>
{
    public int SubmissionId { get; } = submissionId;

    public class Handler(
        ITimesheetRepository timesheetRepository)
        : IRequestHandler<GetTimesheetReviewQuery, ApiResponse<SupervisorReviewResponseDto>>
    {
        public async Task<ApiResponse<SupervisorReviewResponseDto>> Handle(
            GetTimesheetReviewQuery request,
            CancellationToken cancellationToken)
        {
            var submission = await timesheetRepository.GetSubmissionByIdAsync(request.SubmissionId);
            if (submission == null)
            {
                return ApiHelperResponse.Failed<SupervisorReviewResponseDto>("Submission not found.");
            }

            var entries = await timesheetRepository
                .GetEntriesByMonthAsync(submission.EmployeeId, submission.Year, submission.Month);

            var comments = await timesheetRepository.GetCommentsBySubmissionAsync(submission.Id);

            // Build monthly day cells
            var days = entries
                .GroupBy(e => e.EntryDate)
                .Select(dg => new MonthlyDayCellDto
                {
                    Date = dg.Key.ToString("yyyy-MM-dd"),
                    TotalMinutes = dg.Sum(e => e.DurationMinutes),
                    ProjectMinutes = dg
                        .GroupBy(e => e.Project?.Name ?? string.Empty)
                        .ToDictionary(pg => pg.Key, pg => pg.Sum(e => e.DurationMinutes))
                })
                .OrderBy(d => d.Date)
                .ToList();

            var dayCommentDtos = comments.Select(c => new DayCommentResponseDto
            {
                Date = c.CommentDate.ToString("yyyy-MM-dd"),
                Comment = c.Comment
            }).ToList();

            var result = new SupervisorReviewResponseDto
            {
                SubmissionId = submission.Id,
                EmployeeId = submission.EmployeeId,
                EmployeeName = submission.Employee?.FullName ?? string.Empty,
                Year = submission.Year,
                Month = submission.Month,
                Status = submission.Status switch
                {
                    0 => "Waiting for Approval",
                    1 => "Approved",
                    2 => "Need Revision",
                    _ => "Unknown"
                },
                RevisionNote = submission.RevisionNote,
                ReviewedDate = submission.ReviewedDate?.ToString("yyyy-MM-dd HH:mm"),
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
    public class Handler(ITimesheetRepository timesheetRepository)
        : IRequestHandler<GetProjectListQuery, ApiResponse<List<ProjectDto>>>
    {
        public async Task<ApiResponse<List<ProjectDto>>> Handle(
            GetProjectListQuery request,
            CancellationToken cancellationToken)
        {
            var projects = await timesheetRepository.GetAllProjectsAsync();
            var result = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
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
