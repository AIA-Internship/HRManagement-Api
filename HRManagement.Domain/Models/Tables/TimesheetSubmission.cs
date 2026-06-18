using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Tables;

/// <summary>
/// Represents the monthly timesheet submission record.
/// One submission per employee per month.
/// </summary>
public class TimesheetSubmission : BaseTableModel
{
    public int Id { get; private set; }
    public int EmployeeId { get; private set; }

    /// <summary>The year of the submitted timesheet period.</summary>
    public int Year { get; private set; }

    /// <summary>The month of the submitted timesheet period (1–12).</summary>
    public int Month { get; private set; }

    public DateTime SubmittedDate { get; private set; }

    /// <summary>
    /// 0 = Waiting for Approval,
    /// 1 = Approved,
    /// 2 = Need Revision
    /// </summary>
    public int Status { get; private set; }

    /// <summary>Revision note/comment provided by supervisor when giving revision.</summary>
    public string? RevisionNote { get; private set; }

    public DateTime? ReviewedDate { get; private set; }
    public int? ReviewedBy { get; private set; }

    // Relationships are handled at the application level via IDs for Enterprise Scalability.
    // Not mapped in the physical database schema to support high-scale decoupling.
    [NotMapped]
    public Employee? Employee { get; set; }

    protected TimesheetSubmission() { }

    public TimesheetSubmission(int employeeId, int year, int month, long actionerId)
    {
        EmployeeId = employeeId;
        Year = year;
        Month = month;
        Status = 0; // Waiting for Approval
        SubmittedDate = DateTime.UtcNow.AddHours(7);

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    /// <summary>Re-submits an existing submission after revision.</summary>
    public void Resubmit(long actionerId)
    {
        Status = 0; // Back to Waiting for Approval
        RevisionNote = null;
        ReviewedDate = null;
        ReviewedBy = null;
        SubmittedDate = DateTime.UtcNow.AddHours(7);

        MarkAsModified(actionerId);
    }

    public void Approve(int supervisorId, long actionerId)
    {
        Status = 1;
        ReviewedDate = DateTime.UtcNow.AddHours(7);
        ReviewedBy = supervisorId;

        MarkAsModified(actionerId);
    }

    public void GiveRevision(int supervisorId, string revisionNote, long actionerId)
    {
        Status = 2;
        RevisionNote = revisionNote;
        ReviewedDate = DateTime.UtcNow.AddHours(7);
        ReviewedBy = supervisorId;

        MarkAsModified(actionerId);
    }
}
