namespace HRManagement.Domain.Models.Payload;

public record SaveOrSubmitSelfAssessmentPayload
{
    public bool IsDraft { get; init; }
    public List<SelfAssessmentAnswerPayload> Answers { get; init; } = new();
}

public record SelfAssessmentAnswerPayload
{
    public int AssessmentQuestionId { get; init; }
    public string? TextValue { get; init; }
    public int? RatingValue { get; init; }
}