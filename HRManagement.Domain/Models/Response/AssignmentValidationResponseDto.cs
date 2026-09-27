

namespace HRManagement.Domain.Models.Response
{
    public record AssignmentValidationResponseDto(
        int Id,
        string Status,
        int FillerId,
        string AnswerType,
        Dictionary<int, string> ValidQuestions // Key: QuestionId, Value: QuestionType
    );
}
