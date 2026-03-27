<<<<<<< HEAD
=======
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
using HRManagement.Api.Domain.Models.Tables;

namespace HRManagement.Api.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<bool> IsEmailUniqueAsync(string email);
    Task<bool> IsFullNameUniqueAsync(string fullName, int? excludeEmployeeId = null);
    Task<bool> IsPersonalEmailUniqueAsync(string personalEmail, int? excludeEmployeeId = null);
    Task<bool> IsPhoneNumberUniqueAsync(string phoneNumber, int? excludeEmployeeId = null);
    Task<bool> IsNikUniqueAsync(string nik, int? excludeEmployeeId = null);
<<<<<<< HEAD
    Task AddEmployeeAsync(User user, Employee employee, EmploymentInformation? employmentInformation = null, IEnumerable<EmergencyContact>? emergencyContacts = null);
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee?> GetByIdAsync(int id);
    Task UpdateEmployeeAsync(Employee employee);
=======
    Task AddEmployeeAsync(User user, Employee employee);
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByDisplayIdAsync(string displayId);
    Task UpdateEmployeeAsync(Employee employee);
    Task<string?> GetLastEmployeeDisplayIdAsync();
    Task<List<SupervisorLookupDto>> GetSupervisorLookupAsync(CancellationToken cancellationToken = default);
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
}
