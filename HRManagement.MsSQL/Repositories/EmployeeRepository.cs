using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;

using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace HRManagement.MsSQL.Repositories;

public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<bool> IsUniqueAsync<TProperty>(
        Expression<Func<Employee, TProperty>> propertySelector, 
        TProperty value, 
        int? excludeId = null)
    {
        var query = _dbContext.AsQueryable();
        
        if (excludeId.HasValue) 
            query = query.Where(e => e.Id != excludeId.Value);
        
        var parameter = Expression.Parameter(typeof(Employee), "e");
        var property = Expression.Invoke(propertySelector, parameter);
        var constant = Expression.Constant(value);
        var body = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<Employee, bool>>(body, parameter);
        return !await query.AnyAsync(lambda);
    }
    
    public async Task AddEmployeeAsync(Users user, Employee employee)
    {
        await dbContext.Users.AddAsync(user);
        await dbContext.Employees.AddAsync(employee);
        
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        await dbContext.SaveChangesAsync();
    }
    
    public async Task<List<EmployeeUpdateRequest>> GetPendingUpdateRequestsAsync()
    {
        return await _sqldbContext.Set<EmployeeUpdateRequest>()
            .Include(r => r.Employee)
            .Where(r => r.Status == 0)
            .ToListAsync();
    }

    public async Task<List<EmployeeListResponseDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
    {
        var query = from e in _dbContext.AsNoTracking()
                    where e.IsActive

                    // Mengambil employment yang aktif HANYA SEKALI
                    let activeEmployment = e.EmploymentInformations.FirstOrDefault(ei => ei.StatusCode == 1)

                    // Melakukan lookup employment type (asumsi berada di DbContext yang sama)
                    let employmentType = _sqldbContext.Set<Lookup>()
                        .Where(l => l.Category == "EMPLOYMENT_TYPE" &&
                                    l.Value == activeEmployment.TypeCode &&
                                    l.IsActive)
                        .Select(l => l.DisplayName)
                        .FirstOrDefault()

                    // Mapping ke DTO
                    select new EmployeeListResponseDto(
                        e.FullName,
                        activeEmployment != null ? activeEmployment.DisplayId : "",
                        employmentType ?? "",
                        activeEmployment != null ? activeEmployment.DepartmentName : "",
                        activeEmployment != null ? activeEmployment.PositionName : ""
                    );

        var finalQuery = await query.ToListAsync(cancellationToken);

        return finalQuery;
    }

    public async Task<EmployeeProfileResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var query = _dbContext
           .AsNoTracking()
           .Where(e => e.IsActive && e.EmployeeEmail == email);

        var finalQuery = query
            .Select(e => new EmployeeProfileResponseDto(
                e.FullName,
                e.Gender,
                e.PersonalEmail,
                e.EmployeeEmail,
                e.CurrentAddress,
                e.CurrentCity,
                e.CurrentProvince,
                e.CurrentPostalCode,
                e.ResidentialAddress,
                e.ResidentialCity,
                e.ResidentialProvince,
                e.ResidentialPostalCode,
                e.MobilePhone,
                e.NIK,
                e.BirthPlace,
                e.BirthDate,
                _sqldbContext.Set<Lookup>()
                    .Where(l => l.Category == "MARITAL_STATUS" && l.Value == e.MaritalStatus && l.IsActive)
                    .Select(l => l.DisplayName)
                    .FirstOrDefault() ?? "",
                e.IsActive,
                e.EmploymentInformations
                    .Where(ei => ei.StatusCode == 1)
                    .Select(ei => new EmploymentInformationDto(
                        ei.StartDate,
                        ei.DisplayId,
                        _sqldbContext.Set<Lookup>()
                            .Where(l => l.Category == "EMPLOYMENT_TYPE" && l.Value == ei.TypeCode && l.IsActive)
                            .Select(l => l.DisplayName)
                            .FirstOrDefault() ?? "",
                        ei.DepartmentName,
                        ei.PositionName,
                        ei.SupervisorId,
                        ei.SupervisorName
                    ))
                    .FirstOrDefault(),
                e.EmergencyContacts.Select(ec => new EmergencyContactDto(
                    ec.ContactName,
                    ec.ContactRelationship,
                    ec.ContactPhone
                )).ToList()
            ));

        return await finalQuery.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Employee?> GetByDisplayIdAsync(string displayId)
    {
        return await _dbContext
            .Include(e => e.EmploymentInformation)
                .ThenInclude(ei => ei!.Supervisor)
            .Include(e => e.EmergencyContacts)
            .FirstOrDefaultAsync(e => e.EmploymentInformation!.DisplayId == displayId);
    }

    public async Task<string?> GetLastEmployeeDisplayIdAsync()
    {
        return await _sqldbContext.Set<EmploymentInformation>()
            .Where(e => e.DisplayId != null && e.DisplayId.StartsWith("E"))
            .OrderByDescending(e => e.Id)
            .Select(e => e.DisplayId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SupervisorLookupResponseDto>> GetSupervisorLookupAsync(CancellationToken cancellationToken = default)
    {
        var supervisorRole = await _sqldbContext.Set<Roles>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == "Supervisor", cancellationToken);

        if (supervisorRole == null)
        {
            return new List<SupervisorLookupResponseDto>();
        }

        return await _dbContext
            .Include(e => e.EmploymentInformation)
            .AsNoTracking()
            .Where(e => e.RoleId == supervisorRole.Id && e.IsActive)
            .Select(e => new SupervisorLookupResponseDto(
                e.EmploymentInformation!.DisplayId,
                e.FullName))
            .ToListAsync(cancellationToken);
    }
}
