using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class AssessmentGroupConfiguration : IEntityTypeConfiguration<AssessmentGroup>
{
    public void Configure(EntityTypeBuilder<AssessmentGroup> builder)
    {
        builder.ToTable("AssessmentGroups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name);

        builder.HasOne(x => x.Assessment)
            .WithMany(x => x.Groups)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}