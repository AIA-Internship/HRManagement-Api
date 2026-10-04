using System;
using System.Collections.Generic;

namespace HRManagement.Domain.Models.Payload;

public record UpdatePerformanceReviewPlanPayload
(
    string Name,
    string PeriodType,
    DateTime StartDate,
    DateTime EndDate,
    int DurationInMonth,
    int MinReviewDurationInDays,
    string Status,

    List<CreateAssessmentPayload> Assessments,

    List<CreateScoreWeightPayload> ScoreWeights
);

