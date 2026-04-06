using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveBalance;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeUpdateRequest> EmployeeUpdateRequests { get; set; }
    public DbSet<SystemLookup> SystemLookups { get; set; }
    public DbSet<LeaveConfig> LeaveTableConfig { get; set; }
    public DbSet<LeaveBalanceModel> LeaveBalance { get; set; }
    public DbSet<EmployeeAttachment> EmployeeAttachments { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
