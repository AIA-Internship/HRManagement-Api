namespace HRManagement.Api.Application.EmployeeDtos.Queries.Dto;

public class EmployeeListItemDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
}
