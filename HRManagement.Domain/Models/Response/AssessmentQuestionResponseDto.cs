namespace HRManagement.Domain.Models.Response;

public record AssessmentQuestionResponseDto
(
    int Id,
    int AssessmentId,
    string QuestionText,
    int QuestionOrder
);