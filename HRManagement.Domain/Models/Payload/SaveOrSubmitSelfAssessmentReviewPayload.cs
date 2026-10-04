namespace HRManagement.Domain.Models.Payload
{
    public record SaveOrSubmitSelfAssessmentReviewPayload(
        int IntervalId,
        int SubjectId,
        bool IsDraft,
        List<ReviewScoreDto> ReviewScores
    );

    public record ReviewScoreDto(
        int QuestionId,
        decimal? SupervisorRating
    );
}