using System.Linq.Expressions;

using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Application.EmployeeDtos.Queries.Dto;

namespace HRManagement.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<bool> IsUniqueAsync<TProperty>(Expression<Func<Employee, TProperty>> propertySelector, TProperty value, int? excludeId = null);
    Task AddEmployeeAsync(Users user, Employee employee);
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByDisplayIdAsync(string displayId);
    Task UpdateEmployeeAsync(Employee employee);
    Task<string?> GetLastEmployeeDisplayIdAsync();
    Task<List<SupervisorLookupDto>> GetSupervisorLookupAsync(CancellationToken cancellationToken = default);
}
