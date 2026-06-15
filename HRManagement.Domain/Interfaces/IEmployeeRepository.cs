using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;

using System.Linq.Expressions;

namespace HRManagement.Domain.Interfaces;

public interface IEmployeeRepository : IBaseRepository<Employee>
{
    Task<bool> IsUniqueAsync<TProperty>(Expression<Func<Employee, TProperty>> propertySelector, TProperty value, int? excludeId = null);

    Task<List<EmployeeListResponseDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
    Task<EmployeeProfileResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Employee?> GetByDisplayIdAsync(string displayId, CancellationToken cancellationToken = default);
    Task<string?> GetLastEmployeeDisplayIdAsync(CancellationToken cancellationToken = default);
    Task<List<SupervisorLookupResponseDto>> GetSupervisorLookupAsync(CancellationToken cancellationToken = default);

    Task AddEmployeeAsync(Users user, Employee employee);
    Task UpdateEmployeeAsync(Employee employee);
}
