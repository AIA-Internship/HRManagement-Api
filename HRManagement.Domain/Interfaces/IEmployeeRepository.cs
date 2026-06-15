using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;

using System.Linq.Expressions;

namespace HRManagement.Domain.Interfaces;

public interface IEmployeeRepository : IBaseRepository<Employee>
{
    Task<bool> IsUniqueAsync<TProperty>(Expression<Func<Employee, TProperty>> propertySelector, TProperty value, int? excludeId = null);

    Task<List<EmployeeListResponseDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
    Task<EmployeeProfileResponseDto?> GetProfileByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<EmployeeProfileResponseDto?> GetProfileByDisplayIdAsync(string displayId, CancellationToken cancellationToken = default);
    Task<List<SupervisorLookupResponseDto>> GetSupervisorLookupAsync(CancellationToken cancellationToken = default);

    Task<EmploymentInformation?> GetEmploymentInformationByDisplayIdAsync(string displayId, CancellationToken cancellationToken = default);

    Task<string?> GetLastEmployeeDisplayIdAsync(CancellationToken cancellationToken = default);

    Task AddEmployeeUpdateRequestAsync(EmployeeUpdateRequest entity, CancellationToken ct);
    Task AddEmployeeAttachmentsAsync(List<EmployeeAttachment> entities, CancellationToken ct);
}
