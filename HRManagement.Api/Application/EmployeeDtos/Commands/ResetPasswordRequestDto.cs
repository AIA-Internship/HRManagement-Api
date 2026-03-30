namespace HRManagement.Api.Application.EmployeeDtos.Commands;

public record ResetPasswordRequestDto(string Email, string NewPassword, string ConfirmPassword);