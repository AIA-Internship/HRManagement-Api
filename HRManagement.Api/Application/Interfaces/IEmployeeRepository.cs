using HRManagement.Api.Domain.Models.Tables;

namespace HRManagement.Api.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<bool> IsEmailUniqueAsync(string email);
    Task<bool> IsFullNameUniqueAsync(string fullName, int? excludeEmployeeId = null);
    Task<bool> IsPersonalEmailUniqueAsync(string personalEmail, int? excludeEmployeeId = null);
    Task<bool> IsPhoneNumberUniqueAsync(string phoneNumber, int? excludeEmployeeId = null);
    Task<bool> IsNikUniqueAsync(string nik, int? excludeEmployeeId = null);
    Task AddEmployeeAsync(User user, Employee employee);
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee?> GetByIdAsync(int id);
    Task UpdateEmployeeAsync(Employee employee);
}
