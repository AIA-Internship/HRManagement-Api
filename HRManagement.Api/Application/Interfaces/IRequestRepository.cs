using HRManagement.Api.Domain.Models.Tables;

namespace HRManagement.Api.Application.Interfaces;

public interface IRequestRepository
{
    Task UpdateRequestStatusAsync(EmployeeUpdateRequest request);
    
    Task<List<EmployeeUpdateRequest>> GetEmployeeUpdateRequestAsync(int? status);
    
    Task SubmitUpdateRequestAsync(EmployeeUpdateRequest request);
    
    Task<EmployeeUpdateRequest?> GetEmployeeUpdateRequestByIdAsync(int id);
}