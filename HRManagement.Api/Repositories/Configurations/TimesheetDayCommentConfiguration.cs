using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

public class TimesheetDayCommentConfiguration : IEntityTypeConfiguration<TimesheetDayComment>
{
    public void Configure(EntityTypeBuilder<TimesheetDayComment> builder)
    {
        builder.ToTable("TimesheetDayComments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ts_comment_id");

        builder.Property(c => c.SubmissionId)
            .HasColumnName("ts_comment_submission_id")
            .IsRequired();

        builder.Property(c => c.CommentDate)
            .HasColumnName("ts_comment_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(c => c.Comment)
            .HasColumnName("ts_comment_text")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(c => c.IsDeleted).HasColumnName("ts_comment_is_deleted");
        builder.Property(c => c.CreatedBy).HasColumnName("ts_comment_created_by");
        builder.Property(c => c.CreatedUtcDate).HasColumnName("ts_comment_created_date");
        builder.Property(c => c.ModifiedBy).HasColumnName("ts_comment_modified_by");
        builder.Property(c => c.ModifiedUtcDate).HasColumnName("ts_comment_modified_date");

        // No physical DB FK for enterprise-grade performance and decoupling.
        // Managed via SubmissionId.
        
        // One comment per day per submission
        builder.HasIndex(c => new { c.SubmissionId, c.CommentDate }).IsUnique();
    }
}
