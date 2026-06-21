namespace HRManagement.Domain.Models.Response;

public record PerformanceReviewPlanResponseDto
(
    int Id,
    string Name,
    string PeriodType,
    int DurationInMonth,
    int MinReviewDurationInDays,
    DateTime StartDate,
    DateTime EndDate,
    string Status
);