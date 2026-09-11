using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Response
{
    public record InternPerformancePeriodScoreResponseDto(
        int Period,
        string PeriodLabel,
        decimal TechScore,
        decimal SoftSkillScore,
        decimal SelfAssessmentScore,
        decimal PeerReviewScore,
        decimal OverallScore
    );

}
