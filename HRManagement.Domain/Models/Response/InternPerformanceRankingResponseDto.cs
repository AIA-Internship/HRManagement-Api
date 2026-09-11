using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Response
{
    public record InternPerformanceRankingResponseDto(
        long InternId,
        string FullName,
        string InternRole,
        decimal AverageOverallScore,
        int Rank
    );
}
