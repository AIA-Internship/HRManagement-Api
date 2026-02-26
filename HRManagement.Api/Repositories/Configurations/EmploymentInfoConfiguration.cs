using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

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

        builder.Property(e => e.SupervisorName)
            .HasColumnName("employment_supervisor_name")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.HasOne(e => e.Employee)
            .WithOne(emp => emp.EmploymentInformation)
            .HasForeignKey<EmploymentInformation>(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
}