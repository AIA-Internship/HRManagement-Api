using HRManagement.Domain.Models.Response;

namespace HRManagement.Domain.Interfaces;

public interface IAssessmentQuestionRepository
{
    Task<List<AssessmentQuestionResponseDto>> GetByAssessmentIdAsync(
        int assessmentId,
        CancellationToken cancellationToken);
}