namespace HRManagement.Domain.Models.Response
{
    public record SelfAssessmentDto
    (
        int AssessmentId,

        string Role,

        string AnswerType,

        string? RatingDescription,

        List<AssessmentQuestionResponseDto> Questions,

        List<EmployeeListResponseDto> Receivers
    );
}