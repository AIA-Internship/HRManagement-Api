using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Response;

namespace HRManagement.Domain.Interfaces;

public interface IPerformanceReviewPlanRepository
{
    Task<PerformanceReviewPlanDetailResponseDto?> GetPlanByIdAsync(int planId, CancellationToken cancellationToken);
    Task<List<PerformanceReviewPlanResponseDto>> GetAllPlansAsync(CancellationToken cancellationToken);
    Task<List<PerformanceReviewPlanScoreWeightResponseDto>> GetScoreWeightConfigurationsAsync(int planId,CancellationToken cancellationToken);
    Task<EmployeeOngoingPerformanceReviewPlanResponseDto?> GetEmployeeOngoingPerformanceReviewPlanAsync(int fillerId, CancellationToken cancellationToken);

    Task AddPerformanceReviewPlan(CreatePerformanceReviewPlanPayload payload, int actionerId, CancellationToken cancellationToken);
}