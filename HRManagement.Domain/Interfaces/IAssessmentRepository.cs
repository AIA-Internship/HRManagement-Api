using HRManagement.Domain.Models.Response;
namespace HRManagement.Domain.Interfaces
{
    public interface IAssessmentRepository
    {
        Task<List<SelfAssessmentDto>> GetSelfAssessmentsByPlanIdAsync(int planId,CancellationToken cancellationToken);

        Task<List<PeerReviewDto>>GetPeerReviewsByPlanIdAsync(int planId,CancellationToken cancellationToken);

        Task<List<SupervisorAssessmentDto>>GetSupervisorAssessmentsByPlanIdAsync(int planId,CancellationToken cancellationToken);

    }
}
