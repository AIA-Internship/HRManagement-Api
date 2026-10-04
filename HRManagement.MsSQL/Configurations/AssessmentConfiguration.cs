using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AnswerType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AssessmentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.FillerJobTitle)
            .HasMaxLength(255);

        builder.Property(x => x.SubjectJobTitle)
            .HasMaxLength(255);

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.Assessments)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);


    }
}