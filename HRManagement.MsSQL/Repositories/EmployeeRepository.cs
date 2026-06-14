using HRManagement.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Domain.Interfaces;
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

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        return await _dbContext
            .AsNoTracking()
            .Include(e => e.EmploymentInformation)
            .Where(e => e.IsActive == true)
            .ToListAsync();
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await _dbContext
            .Include(e => e.EmploymentInformation)
                .ThenInclude(ei => ei!.Supervisor)
            .Include(e => e.EmergencyContacts)
            .FirstOrDefaultAsync(u => u.EmployeeEmail == email);
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _dbContext
            .Include(e => e.EmploymentInformation)
                .ThenInclude(ei => ei!.Supervisor)
            .Include(e => e.EmergencyContacts)
            .FirstOrDefaultAsync(e => e.Id == id);
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
