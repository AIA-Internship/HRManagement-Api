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
    }
}