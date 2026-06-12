using HRManagement.Api.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class EmploymentInfoConfiguration : IEntityTypeConfiguration<EmploymentInformation>
{
    public void Configure(EntityTypeBuilder<EmploymentInformation> builder)
    {
        builder.ToTable("EmploymentInformation");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("employment_id");
        
        builder.Property(e => e.EmployeeId)
            .HasColumnName("emp_id")
            .IsRequired();
        
        builder.Property(e => e.EmploymentStatus)
            .HasColumnName("employment_status")
            .IsRequired();
        
        builder.Property(e => e.StartDate)
            .HasColumnName("employment_start_date")
            .HasColumnType("date");

        builder.Property(e => e.EmploymentType)
            .HasColumnName("employment_type")
            .IsRequired();
        
        builder.Property(e => e.Department)
            .HasColumnName("employment_department")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.Position)
            .HasColumnName("employment_position")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.SupervisorId)
            .HasColumnName("employment_supervisor_id");

        builder.HasIndex(e => e.SupervisorId);

        builder.Property(e => e.EmployeeDisplayId)
            .HasColumnName("employee_display_id")
            .HasMaxLength(10)
            .IsRequired();
        
        builder.HasIndex(e => e.EmployeeDisplayId)
            .IsUnique();
        
        builder.HasIndex(e => e.EmployeeId);
        
        builder.HasOne(e => e.Employee)
            .WithOne(emp => emp.EmploymentInformation)
            .HasForeignKey<EmploymentInformation>(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Supervisor)
            .WithMany()
            .HasForeignKey(e => e.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
}