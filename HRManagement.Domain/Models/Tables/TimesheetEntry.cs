using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables;

/// <summary>
/// Represents a single timesheet entry row for a specific date.
/// One entry = one project worked on per day.
/// </summary>
public class TimesheetEntry : BaseTable
{
    public int Id { get; private set; }
    public int EmployeeId { get; private set; }
    public DateOnly EntryDate { get; private set; }

    /// <summary>Working duration in minutes.</summary>
    public int DurationMinutes { get; private set; }

    public int ProjectId { get; private set; }
    public string ApplicationUsed { get; private set; } = string.Empty;
    public string TaskDescription { get; private set; } = string.Empty;
    public int ProjectLeadId { get; private set; }

    /// <summary>0 = Office, 1 = WFH, 2 = Meeting Room</summary>
    public int Location { get; private set; }

    /// <summary>working, half_day, off</summary>
    [NotMapped]
    public string DayType { get; private set; } = "working";

    // Relationship status: Logic-only. Not mapped in the physical database schema.
    [NotMapped]
    public Employee Employee { get; set; } = null!;
    [NotMapped]
    public TimesheetProject Project { get; set; } = null!;
    [NotMapped]
    public Employee ProjectLead { get; set; } = null!;

    protected TimesheetEntry() { }

    public TimesheetEntry(
        int employeeId,
        DateOnly entryDate,
        int durationMinutes,
        int projectId,
        string applicationUsed,
        string taskDescription,
        int projectLeadId,
        int location,
        int actionerId,
        string dayType = "working")
    {
        EmployeeId = employeeId;
        EntryDate = entryDate;
        DurationMinutes = durationMinutes;
        ProjectId = projectId;
        ApplicationUsed = applicationUsed;
        TaskDescription = taskDescription;
        ProjectLeadId = projectLeadId;
        Location = location;
        DayType = dayType;

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void UpdateDetails(
        int durationMinutes,
        int projectId,
        string applicationUsed,
        string taskDescription,
        int projectLeadId,
        int location,
        int actionerId)
    {
        DurationMinutes = durationMinutes;
        ProjectId = projectId;
        ApplicationUsed = applicationUsed;
        TaskDescription = taskDescription;
        ProjectLeadId = projectLeadId;
        Location = location;

        MarkAsModified(actionerId);
    }
}



