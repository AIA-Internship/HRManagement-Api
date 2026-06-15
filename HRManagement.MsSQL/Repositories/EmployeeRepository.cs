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
    
    public async Task AddEmployeeUpdateRequestAsync(EmployeeUpdateRequest entity, CancellationToken ct)
    {
        await _sqldbContext.Set<EmployeeUpdateRequest>().AddAsync(entity, ct);
    }

    public async Task AddEmployeeAttachmentsAsync(List<EmployeeAttachment> entities, CancellationToken ct)
    {
        await _sqldbContext.Set<EmployeeAttachment>().AddRangeAsync(entities, ct);
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
                    let activeEmployment = e.EmploymentInformation

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

    public async Task<EmployeeProfileResponseDto?> GetProfileByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var query = from e in _dbContext.AsNoTracking()
                    where e.IsActive && e.EmployeeEmail == email

                    let maritalStatusName = _sqldbContext.Set<Lookup>()
                        .Where(l => l.Category == "MARITAL_STATUS" && l.Value == e.MaritalStatus && l.IsActive)
                        .Select(l => l.DisplayName)
                        .FirstOrDefault()

                    let employmentTypeName = e.EmploymentInformation != null
                        ? _sqldbContext.Set<Lookup>()
                            .Where(l => l.Category == "EMPLOYMENT_TYPE" && l.Value == e.EmploymentInformation.TypeCode && l.IsActive)
                            .Select(l => l.DisplayName)
                            .FirstOrDefault()
                        : null

                    // Mapping Langsung ke DTO
                    select new EmployeeProfileResponseDto(
                        e.Id,
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
                        e.MaritalStatus,
                        maritalStatusName ?? "", // Beri default value jika null
                        e.IsActive,

                        // Mapping 1:1 Employment Information
                        e.EmploymentInformation != null ? new EmploymentInformationDto(
                            e.EmploymentInformation.StartDate,
                            e.EmploymentInformation.DisplayId,
                            employmentTypeName ?? "",
                            e.EmploymentInformation.DepartmentName,
                            e.EmploymentInformation.PositionName,
                            e.EmploymentInformation.SupervisorId,
                            e.EmploymentInformation.SupervisorName
                        ) : null,

                        // Mapping 1:1 Emergency Contact (Tidak perlu lagi .Select().ToList())
                        e.EmergencyContact != null ? new EmergencyContactDto(
                            e.EmergencyContact.ContactName,
                            e.EmergencyContact.ContactRelationship,
                            e.EmergencyContact.ContactPhone
                        ) : null
                    );

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EmployeeProfileResponseDto?> GetProfileByDisplayIdAsync(string displayId, CancellationToken cancellationToken = default)
    {
        var query = from e in _sqldbContext.Set<EmploymentInformation>().AsNoTracking()
                    where !e.IsDeleted && e.StatusCode == 1 && e.DisplayId == displayId

                    let maritalStatusName = _sqldbContext.Set<Lookup>()
                        .Where(l => l.Category == "MARITAL_STATUS" && l.Value == e.Employee.MaritalStatus && l.IsActive)
                        .Select(l => l.DisplayName)
                        .FirstOrDefault()

                    let employmentTypeName = _sqldbContext.Set<Lookup>()
                            .Where(l => l.Category == "EMPLOYMENT_TYPE" && l.Value == e.TypeCode && l.IsActive)
                            .Select(l => l.DisplayName)
                            .FirstOrDefault()

                    // Mapping Langsung ke DTO
                    select new EmployeeProfileResponseDto(
                        e.Employee.Id,
                        e.Employee.FullName,
                        e.Employee.Gender,
                        e.Employee.PersonalEmail,
                        e.Employee.EmployeeEmail,
                        e.Employee.CurrentAddress,
                        e.Employee.CurrentCity,
                        e.Employee.CurrentProvince,
                        e.Employee.CurrentPostalCode,
                        e.Employee.ResidentialAddress,
                        e.Employee.ResidentialCity,
                        e.Employee.ResidentialProvince,
                        e.Employee.ResidentialPostalCode,
                        e.Employee.MobilePhone,
                        e.Employee.NIK,
                        e.Employee.BirthPlace,
                        e.Employee.BirthDate,
                        e.Employee.MaritalStatus,
                        maritalStatusName ?? "", // Beri default value jika null
                        e.Employee.IsActive,
                        new EmploymentInformationDto(
                            e.StartDate,
                            e.DisplayId,
                            employmentTypeName ?? "",
                            e.DepartmentName,
                            e.PositionName,
                            e.SupervisorId,
                            e.SupervisorName
                        ),
                        e.Employee.EmergencyContact != null ? new EmergencyContactDto(
                            e.Employee.EmergencyContact.ContactName,
                            e.Employee.EmergencyContact.ContactRelationship,
                            e.Employee.EmergencyContact.ContactPhone
                        ) : null
                    );

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetLastEmployeeDisplayIdAsync(CancellationToken cancellationToken = default)
    {
        return await _sqldbContext.Set<EmploymentInformation>()
            .Where(e => e.DisplayId != null && e.DisplayId.StartsWith("E"))
            .OrderByDescending(e => e.Id)
            .Select(e => e.DisplayId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<SupervisorLookupResponseDto>> GetSupervisorLookupAsync(CancellationToken cancellationToken = default)
    {
        var supervisorRole = await _sqldbContext.Set<Roles>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name.ToLower() == "supervisor", cancellationToken);

        if (supervisorRole == null)
            return new List<SupervisorLookupResponseDto>();

        return await _dbContext
            .AsNoTracking()
            .Where(e => e.RoleId == supervisorRole.Id && e.IsActive)
            .Select(e => new SupervisorLookupResponseDto(
                e.EmploymentInformation.DisplayId,
                e.FullName))
            .ToListAsync(cancellationToken);
    }

    public async Task<EmploymentInformation?> GetEmploymentInformationByDisplayIdAsync(string displayId, CancellationToken cancellationToken = default)
    {
        return await _sqldbContext.Set<EmploymentInformation>()
             .Where(e => e.DisplayId == displayId && e.StatusCode == 1 && !e.IsDeleted)
             .FirstOrDefaultAsync(cancellationToken);
    }
}
