using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Application.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<Users> Users { get; set; }
    public DbSet<Employee> Employee { get; set; }
    public DbSet<EmployeeUpdateRequest> EmployeeUpdateRequest { get; set; }
    public DbSet<EmploymentInformation> EmploymentInformation { get; set; }
    public DbSet<EmergencyContact> EmergencyContact { get; set; }
    public DbSet<Lookup> Lookup { get; set; }
    // Timesheet Module
    public DbSet<TimesheetProject> TimesheetProjects { get; set; }
    public DbSet<TimesheetEntry> TimesheetEntries { get; set; }
    public DbSet<TimesheetSubmission> TimesheetSubmissions { get; set; }
    public DbSet<TimesheetDayComment> TimesheetDayComments { get; set; }
    public DbSet<TodoTask> TodoTasks { get; set; }
    public DbSet<TimesheetHoliday> TimesheetHolidays { get; set; }

    
    public DbSet<EmployeeAttachment> EmployeeAttachment { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<Permission> Permission { get; set; }
    public DbSet<RolePermission> RolePermission { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}




