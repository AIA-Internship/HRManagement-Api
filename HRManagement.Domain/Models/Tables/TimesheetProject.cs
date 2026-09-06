namespace HRManagement.Domain.Models.Tables;

/// <summary>
/// Represents a project that interns can log time against.
/// Managed by supervisors.
/// </summary>
public class TimesheetProject : BaseTable
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    /// <summary>Name of the project leader (free-text, as entered by supervisor).</summary>
    public string ProjectLeader { get; private set; } = string.Empty;

    /// <summary>0 = Running, 1 = Finished</summary>
    public int Status { get; private set; }

    // Navigation collections removed to support enterprise decoupling.
    // Entries are managed via ProjectId in TimesheetEntry.

    protected TimesheetProject() { }

    public TimesheetProject(string name, string? description, string projectLeader, int actionerId)
    {
        Name = name;
        Description = description;
        ProjectLeader = projectLeader;
        Status = 0; // Running

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void UpdateDetails(string name, string? description, string projectLeader, int status, int actionerId)
    {
        Name = name;
        Description = description;
        ProjectLeader = projectLeader;
        Status = status;

        MarkAsModified(actionerId);
    }
}



