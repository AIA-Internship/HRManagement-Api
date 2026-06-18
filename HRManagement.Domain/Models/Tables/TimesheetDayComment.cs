using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Tables;

/// <summary>
/// Represents a supervisor comment on a specific daily entry during revision review.
/// </summary>
public class TimesheetDayComment : BaseTableModel
{
    public int Id { get; private set; }
    public int SubmissionId { get; private set; }
    public DateOnly CommentDate { get; private set; }
    public string Comment { get; private set; } = string.Empty;

    // Logical relationship: No physical navigation property to prevent physical FK creation by EF Core.
    [NotMapped]
    public TimesheetSubmission Submission { get; set; } = null!;

    protected TimesheetDayComment() { }

    public TimesheetDayComment(int submissionId, DateOnly commentDate, string comment, long actionerId)
    {
        SubmissionId = submissionId;
        CommentDate = commentDate;
        Comment = comment;

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void UpdateComment(string comment, long actionerId)
    {
        Comment = comment;
        MarkAsModified(actionerId);
    }
}
