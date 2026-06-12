using HRManagement.Api.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.Application.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<Users> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeUpdateRequest> EmployeeUpdateRequests { get; set; }
    public DbSet<SystemLookup> SystemLookups { get; set; }
    public DbSet<EmployeeAttachment> EmployeeAttachments { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
