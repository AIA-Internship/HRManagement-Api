using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.Models.Tables.MasterRole;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeUpdateRequest> EmployeeUpdateRequests { get; set; }
    public DbSet<EmploymentInformation> EmploymentInformation { get; set; }
    public DbSet<EmergencyContact> EmergencyContacts { get; set; }
    public DbSet<SystemLookup> SystemLookups { get; set; }
    // Timesheet Module
    public DbSet<TimesheetProject> TimesheetProjects { get; set; }
    public DbSet<TimesheetEntry> TimesheetEntries { get; set; }
    public DbSet<TimesheetSubmission> TimesheetSubmissions { get; set; }
    public DbSet<TimesheetDayComment> TimesheetDayComments { get; set; }
    public DbSet<TodoTask> TodoTasks { get; set; }
    public DbSet<TimesheetHoliday> TimesheetHolidays { get; set; }

    
    public DbSet<EmployeeAttachment> EmployeeAttachments { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
