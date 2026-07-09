using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Domain.Interfaces
{
    public interface IFillAssignmentRepository : IBaseRepository<FillAssignment>
    {
        Task<FillAssignmentDetailResponseDto?> GetAssignmentDetailByIdAsync(int assignmentId, CancellationToken cancellationToken);
        Task<List<FillAssignmentDetailResponseDto>> GetPeerAssignmentDetailsByIntervalAsync(int fillerId, int intervalId, CancellationToken cancellationToken);
        Task<List<AssignmentValidationResponseDto>> GetAssignmentsForValidationAsync(int intervalId, int fillerId, CancellationToken cancellationToken);
        Task<AssignmentValidationResponseDto?> GetAssignmentForValidationAsync(int assignmentId, CancellationToken ct);
        Task UpsertAnswersAsync(List<AssessmentAnswer> incomingAnswers, Dictionary<int, string> assignmentAnswerTypes, CancellationToken cancellationToken);
        Task<List<FillAssignment>> GetAssignmentsWithAssessmentsAsync(List<int> assignmentIds, CancellationToken cancellationToken);
    }
}