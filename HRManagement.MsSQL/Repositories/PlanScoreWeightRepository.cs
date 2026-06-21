using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;

using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace HRManagement.MsSQL.Repositories;

public class PlanScoreWeightRepository : BaseRepository<Employee>, IPlanScoreWeightRepository
{
    public PlanScoreWeightRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<List<PlanScoreWeightResponseDto>> GetByPlanIdAndJobTitleAsync(
    int planId,
    string? jobTitle,
    CancellationToken cancellationToken)
    {
        return await _sqldbContext.PlanScoreWeights
            .Where(x =>
                x.PlanId == planId &&
                !x.IsDeleted &&
                (string.IsNullOrWhiteSpace(jobTitle) || x.SubjectJobTitle == jobTitle)
            )
            .GroupBy(x => x.SubjectJobTitle)
            .Select(g => new PlanScoreWeightResponseDto(
                g.Key!,
                g.Select(x => new ScoreWeightItemDto(
                    x.ScoreType,
                    x.Weights
                )).ToList()
            ))
            .ToListAsync(cancellationToken);
    }

}
