namespace HRManagement.Domain.Models.Tables;

public class LeaveAttachment : BaseTable
{
    public int AttachmentId { get; private set; }
    public int LeaveId { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public bool IsActive { get; private set; }

    protected LeaveAttachment() { }

    public LeaveAttachment(
        int leaveId,
        string documentType,
        string fileName,
        string filePath,
        string contentType,
        long fileSize,
        int actionerId
    )
    {
        // Ensure EmployeeId is recorded (uploader/actioner). Previously this was not set
        // which could lead to DB constraints or missing data when saving attachments.
        //EmployeeId = actionerId;
        LeaveId = leaveId;
        DocumentType = documentType;
        FileName = fileName;
        FilePath = filePath;
        ContentType = contentType;
        FileSize = fileSize;
        IsActive = true;

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }
}
