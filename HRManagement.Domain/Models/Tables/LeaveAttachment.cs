namespace HRManagement.Domain.Models.Tables;

public class LeaveAttachment : BaseTable
{
    public int Id { get; private set; }
    public int EmployeeId { get; private set; }
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
