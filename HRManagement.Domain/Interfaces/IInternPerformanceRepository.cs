using HRManagement.Domain.Models.Response;

namespace HRManagement.Domain.Interfaces;

public interface IInternPerformanceRepository
{
    Task<List<InternPerformanceRankingResponseDto>> GetInternPerformanceRankingAsync(long planId,string role,CancellationToken cancellationToken = default);

    Task<InternPerformanceDetailResponseDto?> GetInternPerformanceDetailAsync(long planId,long internId,CancellationToken cancellationToken = default);

    Task<InternPerformancePeriodScoreResponseDto?> GetPeakPerformanceScoreAsync(long planId,long internId,CancellationToken cancellationToken = default);

    Task<InternPerformancePeriodScoreResponseDto?> GetLowestPerformanceScoreAsync(long planId,long internId,CancellationToken cancellationToken = default);

    Task<InternPerformanceAnnualAverageResponseDto?> GetAnnualAverageScoreAsync(long planId,long internId,CancellationToken cancellationToken = default);

}
