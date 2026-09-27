using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AssessmentAnswerConfiguration : IEntityTypeConfiguration<AssessmentAnswer>
{
    public void Configure(EntityTypeBuilder<AssessmentAnswer> builder)
    {
        builder.ToTable("AssessmentAnswers");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Assignment)
            .WithMany()
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AssessmentQuestion)
            .WithMany(x => x.Answers)
            .HasForeignKey(x => x.AssessmentQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.AssignmentId, x.AssessmentQuestionId }).IsUnique();

        // FIX: Unbind User navigation from duplicate foreign keys, or ignore User on AssessmentAnswer
        builder.Ignore(x => x.User);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}