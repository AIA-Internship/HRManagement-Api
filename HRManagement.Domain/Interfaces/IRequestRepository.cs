using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;

namespace HRManagement.Domain.Interfaces;

public interface IRequestRepository : IBaseRepository<EmployeeUpdateRequest>
{
    Task UpdateRequestStatusAsync(EmployeeUpdateRequest request);
    
    Task<List<EmployeeRequestResponseDto>> GetMyEmployeeUpdateRequestAsync(int? status, int employeeId, CancellationToken cancellationToken = default);
    
    Task SubmitUpdateRequestAsync(EmployeeUpdateRequest request);
    
    Task<EmployeeUpdateRequest?> GetEmployeeUpdateRequestByIdAsync(int id);
}