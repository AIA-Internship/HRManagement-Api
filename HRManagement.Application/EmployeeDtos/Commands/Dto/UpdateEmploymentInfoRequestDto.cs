namespace HRManagement.Application.EmployeeDtos.Commands.Dto;

public class UpdateEmploymentInfoRequestDto
{
    /// <example>1</example>
    public int? EmploymentStatus { get; set; }
    /// <example>2024-01-01</example>
    public DateTime? StartDate { get; set; }
    /// <example>1</example>
    public int? EmploymentType { get; set; }
    /// <example>Information Technology</example>
    public string Department { get; set; } = string.Empty;
    /// <example>Software Engineer</example>
    public string Position { get; set; } = string.Empty;
    /// <example>E0001</example>
    public string? SupervisorDisplayId { get; set; }
    /// <example>E150529</example>
    public string EmployeeDisplayId { get; set; } = string.Empty;
}
