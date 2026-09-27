using HRManagement.Domain.Models.Response;

namespace HRManagement.Domain.Interfaces;

public interface IPerformanceReviewPlanRepository
{
    Task<PerformanceReviewPlanDetailResponseDto?> GetPlanByIdAsync(int planId, CancellationToken cancellationToken);
    Task<List<PerformanceReviewPlanResponseDto>> GetAllPlansAsync(CancellationToken cancellationToken);
    Task<List<PerformanceReviewPlanScoreWeightResponseDto>> GetScoreWeightConfigurationsAsync(int planId,CancellationToken cancellationToken);
    Task<EmployeeOngoingPerformanceReviewPlanResponseDto?> GetEmployeeOngoingPerformanceReviewPlanAsync(int fillerId, CancellationToken cancellationToken);
    Task<List<PerformanceReviewPlanLookupDto>> GetPerformanceReviewPlanLookupsAsync(CancellationToken cancellationToken);
    Task<List<PerformanceReviewIntervalLookupDto>> GetIntervalsByPlanIdAsync(int planId, CancellationToken cancellationToken);
    Task<List<ReviewerLookupDto>> GetSupervisorAssessmentFillerByEmployeeAndIntervalAsync(
    int intervalId,
    int employeeId,
    string assessmentType,
    CancellationToken cancellationToken = default);

}