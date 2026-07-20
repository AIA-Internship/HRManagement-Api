namespace HRManagement.Domain.Models.Response;

public class ReadLeaveAttachmentDto
{
    public int AttachmentId { get; set; }

    public int LeaveId { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public bool IsActive { get; set; }
}