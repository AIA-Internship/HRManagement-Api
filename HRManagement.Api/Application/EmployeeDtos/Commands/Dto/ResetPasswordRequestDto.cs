namespace HRManagement.Api.Application.EmployeeDtos.Commands.Dto;

public record ResetPasswordRequestDto(string Email, string NewPassword, string ConfirmPassword);