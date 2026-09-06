namespace HRManagement.Domain.Models.Payload.TimesheetDtos.Commands.Dto;

// ── Timesheet Entry ──────────────────────────────────────────────────────────

/// <summary>Single row in a daily timesheet entry/edit form.</summary>
public class TimesheetEntryRowDto
{
    /// <summary>Entry row ID; null when creating a new row.</summary>
    public int? Id { get; set; }

    /// <summary>Duration in minutes (e.g., 90 = 1h 30m).</summary>
    public int DurationMinutes { get; set; }

    public int ProjectId { get; set; }

    /// <summary>Comma-separated list of applications used.</summary>
    public string ApplicationUsed { get; set; } = string.Empty;

    public string TaskDescription { get; set; } = string.Empty;

    public int ProjectLeadId { get; set; }

    /// <summary>0 = Office, 1 = WFH, 2 = Meeting Room.</summary>
    public int Location { get; set; }
}

/// <summary>
/// Payload to save (create or replace) all entries for a single date.
/// </summary>
public class SaveDailyTimesheetRequestDto
{
    /// <summary>Date in ISO 8601 format (yyyy-MM-dd).</summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>working, holiday, or off.</summary>
    public string DayType { get; set; } = "working";

    public List<TimesheetEntryRowDto> Entries { get; set; } = new();
}


// ── Timesheet Submission ─────────────────────────────────────────────────────

/// <summary>Payload to submit a monthly timesheet for review.</summary>
public class SubmitTimesheetRequestDto
{
    public int Year { get; set; }
    public int Month { get; set; }
}

// ── Supervisor Review ────────────────────────────────────────────────────────

/// <summary>Payload for supervisor to approve a submission.</summary>
public class ApproveTimesheetRequestDto
{
    public int SubmissionId { get; set; }
}

public class SubmitSupervisorReviewRequestDto
{
    public int SubmissionId { get; set; }
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public List<DayCommentDto> ReviewedDays { get; set; } = new();
}

/// <summary>A single day comment provided by supervisor during revision.</summary>
public class DayCommentDto
{
    /// <summary>Date in ISO 8601 format (yyyy-MM-dd).</summary>
    public string Date { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}

/// <summary>Payload for supervisor to give revision feedback on a submission.</summary>
public class GiveRevisionRequestDto
{
    public int SubmissionId { get; set; }
    public string OverallNote { get; set; } = string.Empty;

    /// <summary>Per-day comments for specific dates that need revision.</summary>
    public List<DayCommentDto> DayComments { get; set; } = new();
}

// ── Projects ─────────────────────────────────────────────────────────────────

/// <summary>Payload for creating a new project (supervisor only).</summary>
public class CreateProjectRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProjectLeader { get; set; } = string.Empty;
}

/// <summary>Payload for updating an existing project (supervisor only).</summary>
public class UpdateProjectRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProjectLeader { get; set; } = string.Empty;

    /// <summary>0 = Running, 1 = Finished.</summary>
    public int Status { get; set; }
}

/// <summary>
/// Single project row in a bulk upsert payload.
/// Id = null means "create new"; Id > 0 means "update existing".
/// </summary>
public class ProjectUpsertItemDto
{
    public int? Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProjectLeader { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

/// <summary>
/// Payload for the Edit Project page — replaces the full project list in one call.
/// Existing IDs not present in the list will be soft-deleted.
/// </summary>
public class BulkUpsertProjectsRequestDto
{
    public List<ProjectUpsertItemDto> Projects { get; set; } = new();
}

// ── To-Do Tasks ──────────────────────────────────────────────────────────────

/// <summary>Payload to create a new to-do task on the dashboard.</summary>
public class CreateTodoTaskRequestDto
{
    public string TaskName { get; set; } = string.Empty;

    /// <summary>Due date in ISO 8601 format (yyyy-MM-dd); optional.</summary>
    public string? DueDate { get; set; }

    /// <summary>0 = Low, 1 = Medium, 2 = High.</summary>
    public int Priority { get; set; }
}

/// <summary>Payload to update a to-do task.</summary>
public class UpdateTodoTaskRequestDto
{
    public string TaskName { get; set; } = string.Empty;
    public string? DueDate { get; set; }
    public int Priority { get; set; }
}


