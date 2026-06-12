namespace HRManagement.Domain.Models.Payload;

public record ResetPasswordPayload(string Email, string NewPassword, string ConfirmPassword);