using HRManagement.Api.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Base;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Users> Users { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<Permission> Permission { get; set; }
    public DbSet<RolePermission> RolePermission { get; set; }

    public DbSet<Employee> Employee { get; set; }
    public DbSet<EmployeeUpdateRequest> EmployeeUpdateRequest { get; set; }
    public DbSet<EmploymentInformation> EmploymentInformation { get; set; }
    public DbSet<SystemLookup> SystemLookup { get; set; }
    public DbSet<EmployeeAttachment> EmployeeAttachment { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}