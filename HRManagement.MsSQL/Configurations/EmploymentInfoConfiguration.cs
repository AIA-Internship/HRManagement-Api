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

        builder.HasOne(e => e.Supervisor)
            .WithMany()
            .HasForeignKey(e => e.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
}