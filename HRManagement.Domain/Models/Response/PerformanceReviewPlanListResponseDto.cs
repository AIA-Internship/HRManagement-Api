namespace HRManagement.Domain.Models.Response;

public record PerformanceReviewPlanListResponseDto
(
    List<PerformanceReviewPlanDetailResponseDto> Items
);