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
            .Include(e => e.EmergencyContacts)
            .FirstOrDefaultAsync(u => u.EmployeeEmail == email);
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await dbContext.Employees
            .Include(e => e.EmploymentInformation)
            .Include(e => e.EmergencyContacts)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
