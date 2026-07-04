using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class FillAssignmentConfiguration : IEntityTypeConfiguration<FillAssignment>
{
    public void Configure(EntityTypeBuilder<FillAssignment> builder)
    {
        builder.ToTable("FillAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne<PerformanceReviewPlan>()
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Interval)
            .WithMany()
            .HasForeignKey(x => x.IntervalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Filler)
            .WithMany()
            .HasForeignKey(x => x.FillerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Assessment)
            .WithMany(p => p.FillAssignments)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}