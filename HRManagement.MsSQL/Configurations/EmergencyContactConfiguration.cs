using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.ToTable("EmergencyContact");
        builder.HasKey(e => e.Id);
        
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}