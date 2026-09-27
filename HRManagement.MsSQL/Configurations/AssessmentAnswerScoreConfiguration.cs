using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AssessmentAnswerScoreConfiguration : IEntityTypeConfiguration<AssessmentAnswerScore>
{
    public void Configure(EntityTypeBuilder<AssessmentAnswerScore> builder)
    {
        builder.ToTable("AssessmentAnswerScores");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score)
            .HasPrecision(5, 2);

        // FIX: Map the explicitly named navigation property 'AssessmentAnswer'
        builder.HasOne(x => x.AssessmentAnswer)
            .WithMany()
            .HasForeignKey(x => x.AssessmentAnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        // FIX: Ignore BaseTable's User navigation property so EF Core won't create a shadow 'UserId' column
        builder.Ignore(x => x.User);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}