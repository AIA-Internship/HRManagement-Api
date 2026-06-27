namespace HRManagement.Domain.Models.Response;

public record PerformanceReviewPlanDetailResponseDto
(
    int Id,
    string Name,
    string PeriodType,
    DateTime StartDate,
    DateTime EndDate,
    int MinReviewDurationInDays,
    int DurationInMonth,
    string Status, 

    List<SelfAssessmentDto> SelfAssessments,

    List<PeerReviewDto> PeerReviews,

    List<SupervisorAssessmentDto> SupervisorAssessments,

    List<PlanScoreWeightResponseDto> ScoreWeightConfigurations
);
