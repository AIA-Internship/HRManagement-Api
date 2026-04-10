using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveBalance;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveResponse;
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
    
    DbSet<EmployeeAttachment> EmployeeAttachments { get; set; }
    public DbSet<LeaveRequestModel> LeaveRequest { get; set; }
    public DbSet<LeaveRequestHistory> LeaveRequestHistory { get; set;  }
    public DbSet<LeaveTableConfig> LeaveTableConfig { get; set; }
    public DbSet<LeaveBalanceModel> leaveBalanceModels { get; set; }


    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
