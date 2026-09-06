namespace HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto;

// ── Projects ──────────────────────────────────────────────────────────────────

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProjectLeader { get; set; } = string.Empty;

    /// <summary>"Running" or "Finished".</summary>
    public string Status { get; set; } = string.Empty;
    public int TotalLoggedMinutes { get; set; }
}

// ── Timesheet Entry ───────────────────────────────────────────────────────────

public class TimesheetEntryResponseDto
{
    public int Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }

    /// <summary>Formatted duration, e.g. "1h 30m".</summary>
    public string DurationFormatted { get; set; } = string.Empty;

    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ApplicationUsed { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public int ProjectLeadId { get; set; }
    public string ProjectLeadName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

// ── Daily View ────────────────────────────────────────────────────────────────

public class DailyTimesheetResponseDto
{
    public string Date { get; set; } = string.Empty;
    public int TotalMinutes { get; set; }
    public string TotalFormatted { get; set; } = string.Empty;
    public string SupervisorRemark { get; set; } = string.Empty;
    public string SubmissionStatus { get; set; } = "Not Submitted";
    public List<TimesheetEntryResponseDto> Entries { get; set; } = new();
}

// ── Weekly View ───────────────────────────────────────────────────────────────

public class WeeklyDayRowDto
{
    public string Date { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalMinutes { get; set; }
    public string TotalFormatted { get; set; } = string.Empty;
    
    public List<string> Projects { get; set; } = new();
    public List<string> AppsUsed { get; set; } = new();
    public List<string> Tasks { get; set; } = new();
    public List<string> Locations { get; set; } = new();
    public string Remark { get; set; } = string.Empty;
    public bool HasComment { get; set; }
}

public class WeeklyTimesheetResponseDto
{
    public string WeekStart { get; set; } = string.Empty;
    public string WeekEnd { get; set; } = string.Empty;
    public int GrandTotalMinutes { get; set; }
    public string GrandTotalFormatted { get; set; } = string.Empty;
    public string SubmissionStatus { get; set; } = "Not Submitted";
    public List<WeeklyDayRowDto> Days { get; set; } = new();
}

// ── Report View ───────────────────────────────────────────────────────────────

public class ReportTimesheetResponseDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string SubmissionStatus { get; set; } = "Not Submitted";
    public int GrandTotalMinutes { get; set; }
    public string GrandTotalFormatted { get; set; } = string.Empty;
    public List<WeeklyDayRowDto> Days { get; set; } = new();
}

// ── Monthly View ──────────────────────────────────────────────────────────────

public class MonthlyDayCellDto
{
    public string Date { get; set; } = string.Empty;

    /// <summary>Key = project name, Value = duration in minutes.</summary>
    public Dictionary<string, int> ProjectMinutes { get; set; } = new();
    public int TotalMinutes { get; set; }
    public string Remark { get; set; } = string.Empty;
    public string SupervisorRemark { get; set; } = string.Empty;
    public List<TimesheetEntryResponseDto> Entries { get; set; } = new();
}

public class MonthlyTimesheetResponseDto
{
    public int Year { get; set; }
    public int Month { get; set; }

    public int? SubmissionId { get; set; }

    /// <summary>"Not Submitted", "Waiting for Approval", "Approved", "Need Revision".</summary>
    public string SubmissionStatus { get; set; } = "Not Submitted";


    public List<MonthlyDayCellDto> Days { get; set; } = new();
}

// ── Submission Status ─────────────────────────────────────────────────────────

public class SubmissionStatusDto
{
    public int? SubmissionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }

    /// <summary>"Not Submitted", "Waiting for Approval", "Approved", "Need Revision".</summary>
    public string Status { get; set; } = "Not Submitted";

    public string? SubmittedDate { get; set; }
    public string? ReviewedDate { get; set; }
    public string? RevisionNote { get; set; }
}

public class SubmissionHistoryItemDto
{
    public int SubmissionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string SubmittedDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RevisionNote { get; set; }
}

// ── Supervisor: Timesheet Approval List ──────────────────────────────────────

public class PendingApprovalItemDto
{
    public int SubmissionId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string SubmittedDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class MissingSubmissionItemDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }

    /// <summary>Number of working days that have passed without any entry.</summary>
    public int OverdueDays { get; set; }
}

public class ApprovalHistoryItemDto
{
    public int SubmissionId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string SubmittedDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RevisionNote { get; set; }
    public string? ReviewedDate { get; set; }
}

// ── Supervisor: Timesheet Review Page ────────────────────────────────────────

public class SupervisorReviewResponseDto
{
    public int SubmissionId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RevisionNote { get; set; }
    public string? ReviewedDate { get; set; }
    public List<DayCommentResponseDto> DayComments { get; set; } = new();
    public List<MonthlyDayCellDto> Days { get; set; } = new();
}

public class DayCommentResponseDto
{
    public string Date { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}

// ── Dashboard ─────────────────────────────────────────────────────────────────

public class DashboardResponseDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public int DaysUntilDeadline { get; set; }
    public SubmissionStatusDto CurrentMonthSubmission { get; set; } = new();
    public List<ProjectSummaryDto> AssignedProjects { get; set; } = new();
    public List<ProjectAllocationDto> ProjectAllocations { get; set; } = new();
    public List<MissingDayDto> MissingDays { get; set; } = new();
    public List<TodoTaskResponseDto> TodoTasks { get; set; } = new();
}

public class ProjectSummaryDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalLoggedMinutes { get; set; }
    public string TotalLoggedFormatted { get; set; } = string.Empty;
}

public class MissingDayDto
{
    public string Date { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
}

public class TodoTaskResponseDto
{
    public int Id { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? DueDate { get; set; }

    /// <summary>"Low", "Medium", "High".</summary>
    public string Priority { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

// ── Supervisor Dashboard ──────────────────────────────────────────────────────

public class SupervisorDashboardResponseDto
{
    public string SupervisorName { get; set; } = string.Empty;
    public int TotalActiveInterns { get; set; }
    public int TotalProjects { get; set; }
    public int TotalRunningProjects { get; set; }
    public int TotalFinishedProjects { get; set; }
    public string ApprovalSummaryCount { get; set; } = string.Empty;
    public string CurrentMonthLabel { get; set; } = string.Empty;
    public List<PendingApprovalItemDto> PendingApprovals { get; set; } = new();
    public List<MissingSubmissionItemDto> MissingSubmissions { get; set; } = new();
    public List<InternHoursBreakdownDto> InternHoursBreakdown { get; set; } = new();
    public List<ProjectAllocationDto> ProjectAllocations { get; set; } = new();
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
}

public class RecentActivityDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string DurationFormatted { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public string EntryDate { get; set; } = string.Empty;
    public string RelativeTime { get; set; } = string.Empty;
}



public class InternHoursBreakdownDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>Key = project name, Value = total minutes.</summary>
    public Dictionary<string, int> ProjectMinutes { get; set; } = new();
    public int TotalMinutes { get; set; }
}

public class ProjectAllocationDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalMinutes { get; set; }
    public double AllocationPercentage { get; set; }
}


