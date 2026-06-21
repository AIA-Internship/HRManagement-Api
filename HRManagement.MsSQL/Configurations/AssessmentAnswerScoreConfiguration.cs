using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class AssessmentAnswerScoreConfiguration : IEntityTypeConfiguration<AssessmentAnswerScore>
{
    public void Configure(EntityTypeBuilder<AssessmentAnswerScore> builder)
    {
        builder.ToTable("AssessmentAnswerScores");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score)
            .HasPrecision(5, 2);

        builder.HasOne<AssessmentAnswer>()
            .WithMany()
            .HasForeignKey(x => x.AssessmentAnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}