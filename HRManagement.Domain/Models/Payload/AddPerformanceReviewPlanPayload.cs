
namespace HRManagement.Domain.Models.Payload;

public record CreatePerformanceReviewPlanPayload
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


public record CreateAssessmentPayload
(
    string AssessmentType,

    string AnswerType,

    string? RatingDescription,

    int? FillerRoleId,

    string? FillerJobTitle,

    int? SubjectRoleId,

    string? SubjectJobTitle,

    List<int> ReceiverIds,

    List<CreateAssessmentQuestionPayload> Questions,

    List<CreateAssessmentGroupPayload>? Groups
);


public record CreateAssessmentQuestionPayload
(
    string QuestionText,
    int QuestionOrder,
    string QuestionType
);


public record CreateAssessmentGroupPayload
(
    string Name,
    string Description,
    List<int> MemberIds
);


public record CreateScoreWeightPayload
(
    int SubjectRoleId,
    string SubjectJobTitle,
    string ScoreType, 
    decimal Weight
);