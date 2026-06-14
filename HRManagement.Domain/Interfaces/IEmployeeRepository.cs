using HRManagement.Domain.Models.Tables;

using System.Linq.Expressions;

namespace HRManagement.Domain.Interfaces;

public interface IEmployeeRepository : IBaseRepository<Employee>
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
