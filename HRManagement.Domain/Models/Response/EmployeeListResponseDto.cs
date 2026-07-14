namespace HRManagement.Domain.Models.Response;

public record EmployeeListResponseDto
(
    string FullName,
    string EmployeeDisplayId,
    string TypeName,
    string Department,
    string Position
);
