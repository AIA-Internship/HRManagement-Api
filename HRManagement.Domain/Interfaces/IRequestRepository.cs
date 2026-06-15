using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;

namespace HRManagement.Domain.Interfaces;

public interface IRequestRepository : IBaseRepository<EmployeeUpdateRequest>
{
    Task<List<EmployeeRequestResponseDto>> GetMyEmployeeUpdateRequestAsync(int? status, int? employeeId, CancellationToken cancellationToken = default);
}