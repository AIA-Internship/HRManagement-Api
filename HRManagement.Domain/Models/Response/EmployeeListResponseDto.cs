namespace HRManagement.Domain.Models.Response;

public class EmployeeListResponseDto
{
    public string EmployeeDisplayId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string EmployeeStatus { get; set; } = string.Empty;
}
