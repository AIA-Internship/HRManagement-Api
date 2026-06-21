using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;

namespace HRManagement.Domain.Interfaces;

public interface IPlanScoreWeightRepository
{
    Task<List<PlanScoreWeightResponseDto>> GetByPlanIdAndJobTitleAsync(int planId, string? jobTitle, CancellationToken cancellationToken);
}