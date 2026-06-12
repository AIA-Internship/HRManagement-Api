namespace HRManagement.Domain.Models.Payload;

public record VerifyForgotPayload(string Email, DateTime DateOfBirth);