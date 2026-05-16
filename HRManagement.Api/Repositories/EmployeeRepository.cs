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
        
        await dbContext.SaveChangesAsync(); // Save to get the employee.Id

        if (employmentInformation != null)
        {
            employmentInformation.EmployeeId = employee.Id;
            await dbContext.EmploymentInformation.AddAsync(employmentInformation);
        }

        if (emergencyContacts != null && emergencyContacts.Any())
        {
            foreach (var contact in emergencyContacts)
            {
                contact.EmployeeId = employee.Id;
                await dbContext.EmergencyContacts.AddAsync(contact);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        if (employee.EmploymentInformation != null)
        {
            // Update or Add logical related record
            var entry = await dbContext.EmploymentInformation.AsNoTracking().FirstOrDefaultAsync(i => i.EmployeeId == employee.Id);
            if (entry == null)
            {
                await dbContext.EmploymentInformation.AddAsync(employee.EmploymentInformation);
            }
            else
            {
                dbContext.EmploymentInformation.Update(employee.EmploymentInformation);
            }
        }

        // Emergency contacts are more complex (re-sync)
        if (employee.EmergencyContacts.Any())
        {
            // Simple approach: Delete old, add current (if not using specific IDs)
            foreach(var c in employee.EmergencyContacts)
            {
                if (c.Id == 0) await dbContext.EmergencyContacts.AddAsync(c);
                else dbContext.EmergencyContacts.Update(c);
            }
        }

        await dbContext.SaveChangesAsync();
    }
    
    public async Task<List<EmployeeUpdateRequest>> GetPendingUpdateRequestsAsync()
    {
        var requests = await dbContext.EmployeeUpdateRequests
            .Where(r => r.Status == 0)
            .ToListAsync();

        var empIds = requests.Select(r => r.EmployeeId).ToList();
        var employees = await dbContext.Employees
            .Where(e => empIds.Contains(e.Id))
            .ToListAsync();

        foreach (var req in requests)
        {
            req.Employee = employees.FirstOrDefault(e => e.Id == req.EmployeeId)!;
        }

        return requests;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        var employees = await dbContext.Employees
            .AsNoTracking()
            .Where(e => e.IsActive == true)
            .ToListAsync();

        var empIds = employees.Select(e => e.Id).ToList();
        var allInfo = await dbContext.EmploymentInformation
            .AsNoTracking()
            .Where(i => empIds.Contains(i.EmployeeId))
            .ToListAsync();

        foreach (var emp in employees)
        {
            emp.EmploymentInformation = allInfo.FirstOrDefault(i => i.EmployeeId == emp.Id);
        }

        return employees;
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(u => u.EmployeeEmail == email);

        if (employee != null)
        {
            employee.EmploymentInformation = await dbContext.EmploymentInformation
                .FirstOrDefaultAsync(i => i.EmployeeId == employee.Id);
            
            employee.EmergencyContacts = await dbContext.EmergencyContacts
                .Where(c => c.EmployeeId == employee.Id && !c.IsDeleted)
                .ToListAsync();
        }

        return employee;
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee != null)
        {
            employee.EmploymentInformation = await dbContext.EmploymentInformation
                .FirstOrDefaultAsync(i => i.EmployeeId == employee.Id);
            
            employee.EmergencyContacts = await dbContext.EmergencyContacts
                .Where(c => c.EmployeeId == employee.Id && !c.IsDeleted)
                .ToListAsync();
        }

        return employee;
    }

    public async Task<Employee?> GetByDisplayIdAsync(string displayId)
    {
        var info = await dbContext.EmploymentInformation
            .FirstOrDefaultAsync(ei => ei.EmployeeDisplayId == displayId);
        
        if (info == null) return null;

        return await GetByIdAsync(info.EmployeeId);
    }

    public async Task<string?> GetLastEmployeeDisplayIdAsync()
    {
        return await dbContext.EmploymentInformation
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

        return supervisors.Select(s => {
            var matchingInfo = infos.FirstOrDefault(i => i.EmployeeId == s.Id);
            return new SupervisorLookupDto(
                matchingInfo?.EmployeeDisplayId ?? string.Empty,
                s.FullName);
        }).ToList();
    }
}

