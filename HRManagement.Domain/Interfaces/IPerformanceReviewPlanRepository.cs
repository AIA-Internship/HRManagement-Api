using HRManagement.Domain.Models.Response;

namespace HRManagement.Domain.Interfaces;

public interface IPerformanceReviewPlanRepository
{
    Task<PerformanceReviewPlanResponseDto?> GetPlanByIdAsync(int planId, CancellationToken cancellationToken);
    Task<List<PerformanceReviewPlanResponseDto>> GetAllPlansAsync(CancellationToken cancellationToken);
}