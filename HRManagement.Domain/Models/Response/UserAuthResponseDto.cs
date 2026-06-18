namespace HRManagement.Domain.Models.Response;

public record UserAuthResponseDto
(
    int Id,
    string EmployeeEmail,
    string PasswordHash,
    string RoleName,
    List<string> Permissions);
