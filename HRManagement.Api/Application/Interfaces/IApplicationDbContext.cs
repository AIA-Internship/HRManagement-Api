using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeUpdateRequest> EmployeeUpdateRequests { get; set; }
    public DbSet<SystemLookup> SystemLookups { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
