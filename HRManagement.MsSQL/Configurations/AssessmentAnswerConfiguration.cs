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

        builder.HasOne<FillAssignment>()
            .WithMany()
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AssessmentQuestion)
            .WithMany()
            .HasForeignKey(x => x.AssessmentQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.User)
        .WithMany()
        .HasForeignKey(x => x.ModifiedBy)
        .OnDelete(DeleteBehavior.Restrict);
    }
}