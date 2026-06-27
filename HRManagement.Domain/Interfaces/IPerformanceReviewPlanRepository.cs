using HRManagement.Domain.Models.Response;

namespace HRManagement.Domain.Interfaces;

public interface IPerformanceReviewPlanRepository
{
    Task<PerformanceReviewPlanDetailResponseDto?> GetPlanByIdAsync(int planId, CancellationToken cancellationToken);
    Task<List<PerformanceReviewPlanResponseDto>> GetAllPlansAsync(CancellationToken cancellationToken);
    Task<List<PlanScoreWeightResponseDto>>GetScoreWeightConfigurationsAsync(int planId,CancellationToken cancellationToken);
}