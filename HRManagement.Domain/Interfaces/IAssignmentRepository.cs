using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;

namespace HRManagement.Domain.Interfaces
{
    public interface IAssignmentRepository : IBaseRepository<FillAssignment>
    {
        Task<FillAssignmentDetailResponseDto?> GetSupervisorAssignmentBySubjectAsync(int supervisorId, int subjectId, int intervalId, CancellationToken cancellationToken);
        Task<FillAssignmentDetailResponseDto?> GetAssignmentDetailByIdAsync(int assignmentId, CancellationToken cancellationToken);
        Task<List<FillAssignmentDetailResponseDto>> GetPeerAssignmentDetailsByIntervalAsync(int fillerId, int intervalId, CancellationToken cancellationToken);
        Task<List<AssignmentValidationResponseDto>> GetAssignmentsForValidationAsync(int intervalId, int fillerId, CancellationToken cancellationToken);
        Task<AssignmentValidationResponseDto?> GetAssignmentForValidationAsync(int assignmentId, CancellationToken ct);
        Task UpsertAnswersAsync(List<AssessmentAnswer> incomingAnswers, Dictionary<int, string> assignmentAnswerTypes, CancellationToken cancellationToken);
        Task<List<FillAssignment>> GetAssignmentsWithAssessmentsAsync(List<int> assignmentIds, CancellationToken cancellationToken);
        Task<List<SupervisorFillAssignmentResponseDto>> GetSupervisorAssignmentStatusesAsync(int? intervalId, CancellationToken cancellationToken);
        Task<FillAssignment?> FindAssignmentAsync(
        int planId,
        int intervalId,
        int fillerId,
        int subjectId,
        int assessmentId,
        CancellationToken cancellationToken);

        Task<FillAssignment?> GetAssignmentByIdAsync(int assignmentId, CancellationToken cancellationToken);

        Task AddAssignmentAsync(FillAssignment assignment, CancellationToken cancellationToken);

        Task<PeerReviewMatrixResponseDto?> GetPeerReviewsBySubjectAsync(
        int supervisorId,
        int intervalId,
        int subjectId,
        CancellationToken cancellationToken);

        Task<ReviewAssignment?> GetReviewAssignmentAsync(
        int planId,
        int intervalId,
        int assignmentId,
        int reviewerId,
        CancellationToken cancellationToken);

        Task<List<AssessmentAnswer>> GetTextAnswersForAssignmentAsync(
            int fillAssignmentId,
            CancellationToken cancellationToken);

        Task UpsertAnswerScoresAsync(
        List<AssessmentAnswerScore> incomingScores,
        CancellationToken cancellationToken);

        Task AddReviewAssignmentAsync(
        ReviewAssignment reviewAssignment,
        CancellationToken cancellationToken);

        Task<List<AssessmentAnswer>> GetAnswersByIdsAsync(List<int> answerIds, CancellationToken cancellationToken);

        Task<FillAssignmentDetailResponseDto?> GetSupervisorSelfAssessmentReviewAsync(
        int supervisorId,
        int subjectId,
        int intervalId,
        CancellationToken cancellationToken);

        Task<FillAssignment?> FindSelfAssessmentFillAssignmentAsync(int intervalId, int subjectId, CancellationToken cancellationToken);
        Task<ReviewAssignment?> FindReviewAssignmentAsync(int planId, int intervalId, int assignmentId, int reviewerId, CancellationToken cancellationToken);
        Task<List<AssessmentAnswer>> GetAnswersByAssignmentIdAsync(int assignmentId, CancellationToken cancellationToken);
        Task UpsertAnswerScoresAsync(int reviewAssignmentId, List<AssessmentAnswerScore> newScores, CancellationToken cancellationToken);

        Task<(int? RoleId, string? JobTitle)> GetReviewerMetadataAsync(int employeeId, CancellationToken cancellationToken = default);

        Task<List<FillAssignment>> FindPeerReviewFillAssignmentsAsync(int intervalId, int subjectId, CancellationToken cancellationToken);
        Task<List<AssessmentAnswer>> GetAnswersByAssignmentIdsAsync(List<int> assignmentIds, CancellationToken cancellationToken);
        Task<List<ReviewAssignment>> FindReviewAssignmentsByAssignmentIdsAsync(List<int> assignmentIds, int reviewerId, CancellationToken cancellationToken);
    }
}