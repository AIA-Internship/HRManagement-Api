using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Response;


    public record InternPerformanceDetailResponseDto(
        long InternId,
        string FullName,
        string InternRole,
        string PeriodType,
        int DurationInMonth,
        List<InternPerformancePeriodScoreResponseDto> Periods,
        decimal? AnnualAverageScore,
        InternPerformancePeriodScoreResponseDto? PeakPerformanceScore,
        InternPerformancePeriodScoreResponseDto? LowestPerformanceScore
    );

