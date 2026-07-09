

namespace HRManagement.Domain.Models.Response
{
    public record AssignmentValidationResponseDto
        (
        int Id,
        string Status,
        int FillerId,
        string AnswerType,
        HashSet<int> ValidQuestionIds
    );
}
