namespace HRManagement.Domain.Models.Payload.EmployeeDtos.Commands.Dto;

public record ResetPasswordRequestDto(string Email, string NewPassword, string ConfirmPassword);

