namespace HRManagement.Domain.Models.Payload;

public record UpdateEmploymentInfoPayload
(
    int? EmploymentStatus,
    DateTime? StartDate,
    int? EmploymentType,
    string Department,
    string Position,
    string? SupervisorDisplayId,
    string EmployeeDisplayId
);
