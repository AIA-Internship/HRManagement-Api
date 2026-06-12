namespace HRManagement.Domain.Models.Tables;

public class EmployeeAttachment : BaseTable
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public bool IsActive { get; set; } = true;
}