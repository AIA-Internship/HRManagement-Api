using HRManagement.Api.Domain.Models.Table;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
using Microsoft.EntityFrameworkCore;

using System.Reflection;

namespace HRManagement.Api.Repositories.Base
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<UserModel>().ToTable("User");
            modelBuilder.Entity<UserModel>().HasKey(p => p.UserId);

            modelBuilder.Entity<RoleModel>().ToTable("Role");
            modelBuilder.Entity<RoleModel>().HasKey(p => p.RoleId);

            modelBuilder.Entity<EmployeeModel>().ToTable("Employee");
            modelBuilder.Entity<EmployeeModel>().HasKey(p => p.Id);

            modelBuilder.Entity<LoginActivityModel>().ToTable("LoginActivity");
            modelBuilder.Entity<LoginActivityModel>().HasKey(p => p.Id);

            //modelBuilder.Entity<ViewMemberModel>().ToView("vw_members").HasNoKey();
            modelBuilder.Entity<LeaveRequestModel>().ToTable("LeaveRequest");
            modelBuilder.Entity<LeaveRequestModel>().HasKey(p => p.LeaveId);

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<UserModel> User { get; set; }
        public virtual DbSet<RoleModel> Role { get; set; }
        public virtual DbSet<EmployeeModel> Employee { get; set; }
        public virtual DbSet<LoginActivityModel> LoginActivity { get; set; }
        public virtual DbSet<LeaveRequestModel> LeaveRequest { get; set; }
    }
}
