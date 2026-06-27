namespace HRManagement.Domain.Models.Response
{
    public record SupervisorAssessmentDto
    (
        int AssessmentId,

        string Role,

        string AnswerType,

        string? RatingDescription,

        List<AssessmentQuestionResponseDto> Questions,

        List<EmployeeListResponseDto> Receivers
    );

}