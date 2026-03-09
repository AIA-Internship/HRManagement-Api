using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories.Base;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeUpdateRequest> EmployeeUpdateRequests { get; set; }
    public DbSet<SystemLookup> SystemLookups { get; set; }
    public DbSet<LeaveRequestModel> LeaveRequest { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        //Configuration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}