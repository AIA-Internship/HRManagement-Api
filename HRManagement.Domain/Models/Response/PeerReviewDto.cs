namespace HRManagement.Domain.Models.Response
{
    public record PeerReviewDto
    (
        int AssessmentId,

        string Role,

        string AnswerType,

        string? RatingDescription,

        List<AssessmentQuestionResponseDto> Questions,

        List<AssessmentGroupDto> Groups
    );
}