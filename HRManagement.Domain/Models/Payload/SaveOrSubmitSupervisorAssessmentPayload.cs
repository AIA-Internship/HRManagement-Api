namespace HRManagement.Domain.Models.Payload;

public record SaveOrSubmitSupervisorAssessmentPayload
{
    public int? AssignmentId { get; init; }
    public int PlanId { get; init; }
    public int IntervalId { get; init; }
    public int SubjectId { get; init; }
    public int AssessmentId { get; init; }
    public bool IsDraft { get; init; }
    public List<AssessmentAnswerItemDto> Answers { get; init; } = [];
}

public record AssessmentAnswerItemDto(
    int AssessmentQuestionId,
    string? TextValue,
    int? RatingValue
);