using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class ReviewAssignmentConfiguration : IEntityTypeConfiguration<ReviewAssignment>
{
    public void Configure(EntityTypeBuilder<ReviewAssignment> builder)
    {
        builder.ToTable("ReviewAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne<PerformanceReviewPlan>()
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PerformanceReviewPlanInterval>()
            .WithMany()
            .HasForeignKey(x => x.IntervalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Assignment)
            .WithMany()
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}