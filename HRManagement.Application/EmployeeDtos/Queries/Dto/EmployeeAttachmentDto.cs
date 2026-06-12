namespace HRManagement.Application.EmployeeDtos.Queries.Dto;

public class EmployeeAttachmentDto
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
}