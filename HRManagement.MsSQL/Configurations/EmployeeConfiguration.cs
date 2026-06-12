using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employee");
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.EmploymentInformation)
            .WithOne(e => e.Employee)
            .HasForeignKey<EmploymentInformation>(e => e.EmployeeId);
        
        builder.HasMany(e => e.EmergencyContacts)
            .WithOne(ec => ec.Employee)
            .HasForeignKey(ec => ec.EmployeeId);
    }
}
