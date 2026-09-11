using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Repositories;

public class InternPerformanceRepository: BaseRepository<EmployeeScoreSummary>, IInternPerformanceRepository
{
    public InternPerformanceRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<InternPerformanceRankingResponseDto>> GetInternPerformanceRankingAsync(long planId,string role,CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.UtcNow.Date;

        var plan = await _sqldbContext.PerformanceReviewPlans
            .AsNoTracking()
            .Include(x => x.PerformanceReviewPlanScoreWeights)
            .Where(x =>
                !x.IsDeleted &&
                x.Id == planId &&
                x.Status == "ongoing" &&
                currentDate >= x.StartDate &&
                currentDate <= x.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (plan == null)
        {
            return new List<InternPerformanceRankingResponseDto>();
        }

        var summaries = await _sqldbContext.EmployeeScoreSummaries
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.PlanerId == plan.Id)
            .ToListAsync(cancellationToken);

        if (!summaries.Any())
        {
            return new List<InternPerformanceRankingResponseDto>();
        }

        // Filter berdasarkan role
        var filteredSummaries = summaries
            .Where(x =>
                string.Equals(
                    x.InternRole?.Trim(),
                    role?.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!filteredSummaries.Any())
        {
            return new List<InternPerformanceRankingResponseDto>();
        }

        var internIds = filteredSummaries
            .Select(x => x.InternId)
            .Distinct()
            .ToList();

        var employees = await _sqldbContext.Set<Employee>()
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                internIds.Contains(x.Id))
            .Select(x => new
            {
                Id = (long)x.Id,
                x.FullName
            })
            .ToListAsync(cancellationToken);

        var ranking = filteredSummaries
            .GroupBy(x => new
            {
                x.InternId,
                x.InternRole
            })
            .Select(group =>
            {
                var overallScores = group
                    .Select(x => CalculateOverallScore(
                        x,
                        plan.PerformanceReviewPlanScoreWeights
                            .Where(w => !w.IsDeleted)
                            .ToList()))
                    .ToList();

                return new
                {
                    InternId = group.Key.InternId,
                    InternRole = group.Key.InternRole,
                    AverageOverallScore = overallScores.Any()
                        ? overallScores.Average()
                        : 0
                };
            })
            .Join(
                employees,
                x => x.InternId,
                e => e.Id,
                (x, e) => new InternPerformanceRankingResponseDto(
                    x.InternId,
                    e.FullName,
                    x.InternRole,
                    Math.Round(x.AverageOverallScore, 2),
                    0))
            .OrderByDescending(x => x.AverageOverallScore)
            .ToList();

        return ranking
            .Select((x, index) => new InternPerformanceRankingResponseDto(
                x.InternId,
                x.FullName,
                x.InternRole,
                x.AverageOverallScore,
                index + 1))
            .ToList();
    }

    public async Task<InternPerformanceDetailResponseDto?> GetInternPerformanceDetailAsync(long planId,long internId,CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.UtcNow.Date;

        var plan = await _sqldbContext.PerformanceReviewPlans
            .AsNoTracking()
            .Include(x => x.PerformanceReviewPlanScoreWeights)
            .Where(x =>
                !x.IsDeleted &&
                x.Id == planId )
            .FirstOrDefaultAsync(cancellationToken);

        if (plan == null)
        {
            return null;
        }

        var summaries = await _sqldbContext.EmployeeScoreSummaries
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.PlanerId == plan.Id &&
                x.InternId == internId)
            .OrderBy(x => x.Period)
            .ToListAsync(cancellationToken);

        if (!summaries.Any())
        {
            return null;
        }

        var employee = await _sqldbContext.Employees
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Id == internId)
            .Select(x => new
            {
                Id = (long)x.Id,
                x.FullName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (employee == null)
        {
            return null;
        }

        var weights = plan.PerformanceReviewPlanScoreWeights
            .Where(x => !x.IsDeleted)
            .ToList();

        var periods = summaries
            .Select(x =>
            {
                var overallScore = CalculateOverallScore(
                    x,
                    weights);

                return new InternPerformancePeriodScoreResponseDto(
                    x.Period,
                    GetPeriodLabel(
                        plan.PeriodType,
                        x.Period),
                    x.TechScore,
                    x.SoftSkillScore,
                    x.SelfAssessmentScore,
                    x.PeerReviewScore,
                    Math.Round(overallScore, 2));
            })
            .ToList();

        var average = periods.Any()
            ? periods.Average(x => x.OverallScore)
            : (decimal?)null;

        var peak = periods
            .OrderByDescending(x => x.OverallScore)
            .FirstOrDefault();

        var lowest = periods
            .OrderBy(x => x.OverallScore)
            .FirstOrDefault();

        return new InternPerformanceDetailResponseDto(
            employee.Id,
            employee.FullName,
            summaries.First().InternRole,
            plan.PeriodType,
            plan.DurationInMonth,
            periods,
            average.HasValue
                ? Math.Round(average.Value, 2)
                : null,
            peak,
            lowest);
    }

    public async Task<InternPerformancePeriodScoreResponseDto?> GetPeakPerformanceScoreAsync(long planId,long internId,CancellationToken cancellationToken = default)
    {
        var periods = await GetInternPeriodScoresAsync(
            planId,
            internId,
            cancellationToken);

        return periods
            .OrderByDescending(x => x.OverallScore)
            .FirstOrDefault();
    }

    public async Task<InternPerformancePeriodScoreResponseDto?> GetLowestPerformanceScoreAsync(long planId,long internId,CancellationToken cancellationToken = default)
    {
        var periods = await GetInternPeriodScoresAsync(
            planId,
            internId,
            cancellationToken);

        return periods
            .OrderBy(x => x.OverallScore)
            .FirstOrDefault();
    }

    public async Task<InternPerformanceAnnualAverageResponseDto?> GetAnnualAverageScoreAsync(long planId,long internId,CancellationToken cancellationToken = default)
    {
        var periods = await GetInternPeriodScoresAsync(
            planId,
            internId,
            cancellationToken);

        if (!periods.Any())
        {
            return null;
        }

        var averageOverallScore = Math.Round(
            periods.Average(x => x.OverallScore),
            2);

        return new InternPerformanceAnnualAverageResponseDto(
            internId,
            averageOverallScore);
    }

    private async Task<List<InternPerformancePeriodScoreResponseDto>>GetInternPeriodScoresAsync(long planId,long internId,CancellationToken cancellationToken)
    {
        var currentDate = DateTime.UtcNow.Date;

        var plan = await _sqldbContext.PerformanceReviewPlans
            .AsNoTracking()
            .Include(x => x.PerformanceReviewPlanScoreWeights)
            .Where(x =>
                !x.IsDeleted &&
                x.Id == planId)
            .FirstOrDefaultAsync(cancellationToken);

        if (plan == null)
        {
            return new List<InternPerformancePeriodScoreResponseDto>();
        }

        var summaries = await _sqldbContext.EmployeeScoreSummaries
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.PlanerId == plan.Id &&
                x.InternId == internId)
            .OrderBy(x => x.Period)
            .ToListAsync(cancellationToken);

        var weights = plan.PerformanceReviewPlanScoreWeights
            .Where(x => !x.IsDeleted)
            .ToList();

        return summaries
            .Select(x =>
            {
                var overallScore = CalculateOverallScore(
                    x,
                    weights);

                return new InternPerformancePeriodScoreResponseDto(
                    x.Period,
                    GetPeriodLabel(
                        plan.PeriodType,
                        x.Period),
                    x.TechScore,
                    x.SoftSkillScore,
                    x.SelfAssessmentScore,
                    x.PeerReviewScore,
                    Math.Round(overallScore, 2));
            })
            .ToList();
    }

    private static decimal CalculateOverallScore(EmployeeScoreSummary summary,List<PerformanceReviewPlanScoreWeight> weights)
    {
        var roleWeights = weights
            .Where(x =>
                string.Equals(
                    x.SubjectJobTitle,
                    summary.InternRole,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!roleWeights.Any())
        {
            return 0;
        }

        decimal overallScore = 0;

        foreach (var weight in roleWeights)
        {
            var normalizedScoreType = weight.ScoreType
                .Trim()
                .ToLower();

            decimal score = normalizedScoreType switch
            {
                "technical" => summary.TechScore,
                "soft-skill" => summary.SoftSkillScore,
                "self-assessment" => summary.SelfAssessmentScore,
                "peer-review" => summary.PeerReviewScore,
                _ => 0
            };

            overallScore += score * (weight.Weights / 100);
        }

        return overallScore;
    }

    private static string GetPeriodLabel(string periodType,int period)
    {
        if (string.Equals(
                periodType,
                "Quarterly",
                StringComparison.OrdinalIgnoreCase))
        {
            return $"Q{period}";
        }

        return $"Month {period}";
    }

}