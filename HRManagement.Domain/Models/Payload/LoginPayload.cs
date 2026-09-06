namespace HRManagement.Domain.Models.Payload;

public record LoginPayload
(
    string Email, 
    string Password, 
    bool RememberMe
);
