using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories.Base;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeUpdateRequest> EmployeeUpdateRequests { get; set; }
    public DbSet<EmploymentInformation> EmploymentInformation { get; set; }
    public DbSet<EmergencyContact> EmergencyContacts { get; set; }
    public DbSet<SystemLookup> SystemLookups { get; set; }

    // Timesheet Module (Sharding-ready/Decoupled)
    public DbSet<TimesheetProject> TimesheetProjects { get; set; }
    public DbSet<TimesheetEntry> TimesheetEntries { get; set; }
    public DbSet<TimesheetSubmission> TimesheetSubmissions { get; set; }
    public DbSet<TimesheetDayComment> TimesheetDayComments { get; set; }
    public DbSet<TodoTask> TodoTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}