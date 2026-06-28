

namespace HRManagement.Domain.Models.Response
{
    public record PerformanceReviewPlanIntervalResponseDto
    (
        int Id,
        int PlanId,
        int IntervalNumber,
        DateTime StartDate,
        DateTime DueDate,
        DateTime EndDate,
        string Status
    );
}
