using HRManagement.Api.Domain.Models.Tables;

namespace HRManagement.Application.Interfaces;

public interface IRequestRepository
{
    Task UpdateRequestStatusAsync(EmployeeUpdateRequest request);
    
    Task<List<EmployeeUpdateRequest>> GetEmployeeUpdateRequestAsync(int? status, int? employeeId = null);
    
    Task SubmitUpdateRequestAsync(EmployeeUpdateRequest request);
    
    Task<EmployeeUpdateRequest?> GetEmployeeUpdateRequestByIdAsync(int id);
}