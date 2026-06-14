using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Base;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public virtual DbSet<Users> Users { get; set; }
    public virtual DbSet<Roles> Roles { get; set; }
    public virtual DbSet<Permission> Permission { get; set; }
    public virtual DbSet<RolePermission> RolePermission { get; set; }
    public virtual DbSet<Lookup> Lookup { get; set; }

    public virtual DbSet<Employee> Employee { get; set; }
    public virtual DbSet<EmployeeUpdateRequest> EmployeeUpdateRequest { get; set; }
    public virtual DbSet<EmploymentInformation> EmploymentInformation { get; set; }
    public virtual DbSet<EmployeeAttachment> EmployeeAttachment { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}