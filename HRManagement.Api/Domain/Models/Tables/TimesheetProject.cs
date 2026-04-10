namespace HRManagement.Api.Domain.Models.Tables;

/// <summary>
/// Represents a project that interns can log time against.
/// Managed by supervisors.
/// </summary>
public class TimesheetProject : BaseTableModel
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    /// <summary>0 = Running, 1 = Finished</summary>
    public int Status { get; private set; }

    // Navigation collections removed to support enterprise decoupling.
    // Entries are managed via ProjectId in TimesheetEntry.

    protected TimesheetProject() { }

    public TimesheetProject(string name, string? description, long actionerId)
    {
        Name = name;
        Description = description;
        Status = 0; // Running

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void UpdateDetails(string name, string? description, int status, long actionerId)
    {
        Name = name;
        Description = description;
        Status = status;

        MarkAsModified(actionerId);
    }
}
