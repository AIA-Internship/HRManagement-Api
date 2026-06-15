namespace HRManagement.Domain.Models.Payload;

public record ReviewUpdatePayload
(
    int RequestId,
    bool IsApproved,
    string? Reason
);
