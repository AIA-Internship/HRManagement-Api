using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Base;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Users> Users { get; set; }
    public DbSet<Employee> Employee { get; set; }
    public DbSet<EmployeeUpdateRequest> EmployeeUpdateRequest { get; set; }
    public DbSet<EmploymentInformation> EmploymentInformation { get; set; }
    public DbSet<EmergencyContact> EmergencyContact { get; set; }
    public DbSet<Lookup> Lookup { get; set; }
    public DbSet<EmployeeAttachment> EmployeeAttachment { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<Permission> Permission { get; set; }
    public DbSet<RolePermission> RolePermission { get; set; }

    // Timesheet Module (Sharding-ready/Decoupled)
    public DbSet<TimesheetProject> TimesheetProjects { get; set; }
    public DbSet<TimesheetHoliday> TimesheetHolidays { get; set; }
    public DbSet<TimesheetEntry> TimesheetEntries { get; set; }
    public DbSet<TimesheetSubmission> TimesheetSubmissions { get; set; }
    public DbSet<TimesheetDayComment> TimesheetDayComments { get; set; }
    public DbSet<TodoTask> TodoTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseTable).IsAssignableFrom(entityType.ClrType))
            {
                if (entityType.ClrType.Name.StartsWith("Timesheet") || entityType.ClrType.Name.StartsWith("Todo"))
                {
                    modelBuilder.Entity(entityType.ClrType).Property("CreatedBy").HasConversion<long>();
                    modelBuilder.Entity(entityType.ClrType).Property("ModifiedBy").HasConversion<long>();
                }
                else
                {
                    modelBuilder.Entity(entityType.ClrType).Property("CreatedBy").HasConversion<int>();
                    modelBuilder.Entity(entityType.ClrType).Property("ModifiedBy").HasConversion<int>();
                }
            }
        }
    }
}



