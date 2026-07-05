using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

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

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => new { x.AssignmentId, x.AssessmentQuestionId }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}