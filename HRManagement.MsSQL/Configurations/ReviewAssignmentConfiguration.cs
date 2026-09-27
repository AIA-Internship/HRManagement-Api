using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ReviewAssignmentConfiguration : IEntityTypeConfiguration<ReviewAssignment>
{
    public void Configure(EntityTypeBuilder<ReviewAssignment> builder)
    {
        builder.ToTable("ReviewAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ReviewerJobTitle)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.HasOne(x => x.Interval)
            .WithMany()
            .HasForeignKey(x => x.IntervalId)
            .OnDelete(DeleteBehavior.Restrict);

        // Point .WithMany to the ReviewAssignments property on FillAssignment
        builder.HasOne(x => x.Assignment)
            .WithMany(fa => fa.ReviewAssignments)
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewerRole)
            .WithMany()
            .HasForeignKey(x => x.ReviewerRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Reviewer)
            .WithMany()
            .HasForeignKey(x => x.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.User); // Ignore BaseTable's User property if not linked

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}