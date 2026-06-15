using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class EmployeeUpdateRequestConfiguration : IEntityTypeConfiguration<EmployeeUpdateRequest>
{
    public void Configure(EntityTypeBuilder<EmployeeUpdateRequest> builder)
    {
        builder.ToTable("EmployeeUpdateRequest");
        builder.HasKey(e => e.Id);
        
        builder.HasOne(e => e.Employee)
            .WithMany() 
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);
    }
}