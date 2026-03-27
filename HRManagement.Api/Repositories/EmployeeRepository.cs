using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories;

public class EmployeeRepository(AppDbContext dbContext) : IEmployeeRepository
{

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await dbContext.Users.AnyAsync(u => u.EmployeeEmail == email);
    }

    public async Task<bool> IsFullNameUniqueAsync(string fullName, int? excludeEmployeeId = null)
    {
        var query = dbContext.Employees.AsQueryable();
        if (excludeEmployeeId.HasValue) query = query.Where(e => e.Id != excludeEmployeeId.Value);
        return !await query.AnyAsync(e => e.FullName == fullName);
    }

    public async Task<bool> IsPersonalEmailUniqueAsync(string personalEmail, int? excludeEmployeeId = null)
    {
        var query = dbContext.Employees.AsQueryable();
        if (excludeEmployeeId.HasValue) query = query.Where(e => e.Id != excludeEmployeeId.Value);
        return !await query.AnyAsync(e => e.PersonalEmail == personalEmail);
    }

    public async Task<bool> IsPhoneNumberUniqueAsync(string phoneNumber, int? excludeEmployeeId = null)
    {
        var query = dbContext.Employees.AsQueryable();
        if (excludeEmployeeId.HasValue) query = query.Where(e => e.Id != excludeEmployeeId.Value);
        return !await query.AnyAsync(e => e.PhoneNumber == phoneNumber);
    }

    public async Task<bool> IsNikUniqueAsync(string nik, int? excludeEmployeeId = null)
    {
        var query = dbContext.Employees.AsQueryable();
        if (excludeEmployeeId.HasValue) query = query.Where(e => e.Id != excludeEmployeeId.Value);
        return !await query.AnyAsync(e => e.Nik == nik);
    }

    public async Task AddEmployeeAsync(User user, Employee employee, EmploymentInformation? employmentInformation = null, IEnumerable<EmergencyContact>? emergencyContacts = null)
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
            // But let's just use Update for now if they are tracked.
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
}
