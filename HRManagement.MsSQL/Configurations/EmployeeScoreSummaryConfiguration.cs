using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class EmployeeScoreSummaryConfiguration
    : IEntityTypeConfiguration<EmployeeScoreSummary>
{
    public void Configure(EntityTypeBuilder<EmployeeScoreSummary> builder)
    {
        builder.ToTable("EmployeeScoreSummary");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InternRole)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.TechScore)
            .HasPrecision(5, 2);

        builder.Property(x => x.SoftSkillScore)
            .HasPrecision(5, 2);

        builder.Property(x => x.SelfAssessmentScore)
            .HasPrecision(5, 2);

        builder.Property(x => x.PeerReviewScore)
            .HasPrecision(5, 2);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}