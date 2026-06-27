using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class PlanScoreWeightConfiguration : IEntityTypeConfiguration<PlanScoreWeight>
{
    public void Configure(EntityTypeBuilder<PlanScoreWeight> builder)
    {
        builder.ToTable("PlanScoreWeights");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubjectJobTitle)
            .HasMaxLength(255);

        builder.Property(x => x.ScoreType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Weights)
            .HasPrecision(5, 2);

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.PlanScoreWeights)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

    }
}