using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class AssessmentReceiverConfiguration : IEntityTypeConfiguration<AssessmentReceiver>
{
    public void Configure(EntityTypeBuilder<AssessmentReceiver> builder)
    {
        builder.ToTable("AssessmentReceivers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReceiverType)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(x => x.Assessment)
            .WithMany(x => x.Receivers)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}