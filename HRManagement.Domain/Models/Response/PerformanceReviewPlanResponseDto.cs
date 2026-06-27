
namespace HRManagement.Domain.Models.Response
{
    public record PerformanceReviewPlanResponseDto
    (
        int Id,
        string Name,
        string PeriodType,
        DateTime StartDate,
        DateTime EndDate,
        int MinReviewDurationInDays,
        int DurationInMonth,
        string Status
    );
}
