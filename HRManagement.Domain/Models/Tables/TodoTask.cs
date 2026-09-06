using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables;

/// <summary>
/// Stores intern's personal to-do task items visible on the dashboard.
/// </summary>
public class TodoTask : BaseTable
{
    public int Id { get; private set; }
    public int EmployeeId { get; private set; }
    public string TaskName { get; private set; } = string.Empty;
    public DateOnly? DueDate { get; private set; }

    /// <summary>0 = Low, 1 = Medium, 2 = High</summary>
    public int Priority { get; private set; }

    public bool IsCompleted { get; private set; }

    // Relationship managed via EmployeeId. Logical decoupling for Enterprise systems.
    [NotMapped]
    public Employee? Employee { get; set; }

    protected TodoTask() { }

    public TodoTask(int employeeId, string taskName, DateOnly? dueDate, int priority, int actionerId)
    {
        EmployeeId = employeeId;
        TaskName = taskName;
        DueDate = dueDate;
        Priority = priority;
        IsCompleted = false;

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ToggleCompleted(int actionerId)
    {
        IsCompleted = !IsCompleted;
        MarkAsModified(actionerId);
    }

    public void UpdateDetails(string taskName, DateOnly? dueDate, int priority, int actionerId)
    {
        TaskName = taskName;
        DueDate = dueDate;
        Priority = priority;

        MarkAsModified(actionerId);
    }
}



