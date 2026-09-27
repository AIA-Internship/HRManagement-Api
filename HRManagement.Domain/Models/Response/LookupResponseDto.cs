namespace HRManagement.Domain.Models.Response;

public record LookupResponseDto(int Value, string DisplayName);

public record PerformanceReviewPlanLookupDto
{
    public int PlanId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record PerformanceReviewIntervalLookupDto
{
    public int IntervalId { get; init; }
    public int IntervalNumber { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = string.Empty;
}