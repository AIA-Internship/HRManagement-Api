using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace HRManagement.MsSQL.Configurations;

public class PerformanceReviewPlanIntervalConfiguration : IEntityTypeConfiguration<PerformanceReviewPlanInterval>
{
    public void Configure(EntityTypeBuilder<PerformanceReviewPlanInterval> builder)
    {
        builder.ToTable("PerformanceReviewPlanIntervals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IntervalNumber)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(x => x.DueDate)
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(x => x.FillAssignments)
            .WithOne(fa => fa.Interval)
            .HasForeignKey(fa => fa.IntervalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PerformanceReviewPlan)
            .WithMany(p => p.Intervals)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}