using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Response
{
    public record InternPerformanceAnnualAverageResponseDto(
        long InternId,
        decimal AverageOverallScore
    );

}
