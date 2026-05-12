using System.Linq.Expressions;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories;

public class EmployeeRepository(AppDbContext dbContext) : IEmployeeRepository
{

    public async Task<bool> IsUniqueAsync<TProperty>(
        Expression<Func<Employee, TProperty>> propertySelector, 
        TProperty value, 
        int? excludeId = null)
    {
        var query = dbContext.Employees.AsQueryable();
        
        if (excludeId.HasValue) 
            query = query.Where(e => e.Id != excludeId.Value);
        
        var parameter = Expression.Parameter(typeof(Employee), "e");
        var property = Expression.Invoke(propertySelector, parameter);
        var constant = Expression.Constant(value);
        var body = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<Employee, bool>>(body, parameter);
        return !await query.AnyAsync(lambda);
    }
    
    public async Task AddEmployeeAsync(User user, Employee employee)
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
        return await dbContext.EmployeeUpdateRequests
            .Include(r => r.Employee)
            .Where(r => r.Status == 0)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        return await dbContext.Employees
            .AsNoTracking()
            .Include(e => e.EmploymentInformation)
            .Where(e => e.IsActive == true)
            .ToListAsync();
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await dbContext.Employees
            .Include(e => e.EmploymentInformation)
                .ThenInclude(ei => ei!.Supervisor)
            .Include(e => e.EmergencyContacts)
            .FirstOrDefaultAsync(u => u.EmployeeEmail == email);
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await dbContext.Employees
            .Include(e => e.EmploymentInformation)
                .ThenInclude(ei => ei!.Supervisor)
            .Include(e => e.EmergencyContacts)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee?> GetByDisplayIdAsync(string displayId)
    {
        return await dbContext.Employees
            .Include(e => e.EmploymentInformation)
                .ThenInclude(ei => ei!.Supervisor)
            .Include(e => e.EmergencyContacts)
            .FirstOrDefaultAsync(e => e.EmploymentInformation!.EmployeeDisplayId == displayId);
    }

    public async Task<string?> GetLastEmployeeDisplayIdAsync()
    {
        return await dbContext.EmploymentInformations
            .Where(e => e.EmployeeDisplayId != null && e.EmployeeDisplayId.StartsWith("E"))
            .OrderByDescending(e => e.Id)
            .Select(e => e.EmployeeDisplayId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SupervisorLookupDto>> GetSupervisorLookupAsync(CancellationToken cancellationToken = default)
    {
        var supervisorRole = await dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == "Supervisor", cancellationToken);

        if (supervisorRole == null)
        {
            return new List<SupervisorLookupDto>();
        }

        return await dbContext.Employees
            .Include(e => e.EmploymentInformation)
            .AsNoTracking()
            .Where(e => e.RoleId == supervisorRole.Id && e.IsActive)
            .Select(e => new SupervisorLookupDto(
                e.EmploymentInformation!.EmployeeDisplayId,
                e.FullName))
            .ToListAsync(cancellationToken);
    }
}
