using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class PerformanceReviewPlanConfiguration : IEntityTypeConfiguration<PerformanceReviewPlan>
{
    public void Configure(EntityTypeBuilder<PerformanceReviewPlan> builder)
    {
        builder.ToTable("PerformanceReviewPlans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.PeriodType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnType("date");

        builder.Property(x => x.EndDate)
            .HasColumnType("date");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}