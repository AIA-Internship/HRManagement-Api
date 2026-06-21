using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Repositories;

public class PerformanceReviewPlanRepository : BaseRepository<PerformanceReviewPlan>, IPerformanceReviewPlanRepository
{
    public PerformanceReviewPlanRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<PerformanceReviewPlanResponseDto?> GetPlanByIdAsync(
        int planId,
        CancellationToken cancellationToken)
    {
        return await _sqldbContext.PerformanceReviewPlans
            .Where(x => x.Id == planId && !x.IsDeleted)
            .Select(x => new PerformanceReviewPlanResponseDto(
                x.Id,
                x.Name,
                x.PeriodType,
                x.DurationInMonth,
                x.MinReviewDurationInDays,
                x.StartDate,
                x.EndDate,
                x.Status
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PerformanceReviewPlanResponseDto>> GetAllPlansAsync(
        CancellationToken cancellationToken)
    {
        return await _sqldbContext.PerformanceReviewPlans
            .Where(x => !x.IsDeleted)
            .Select(x => new PerformanceReviewPlanResponseDto(
                x.Id,
                x.Name,
                x.PeriodType,
                x.DurationInMonth,
                x.MinReviewDurationInDays,
                x.StartDate,
                x.EndDate,
                x.Status
            ))
            .ToListAsync(cancellationToken);
    }
}