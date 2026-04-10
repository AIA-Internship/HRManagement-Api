using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

public class TimesheetSubmissionConfiguration : IEntityTypeConfiguration<TimesheetSubmission>
{
    public void Configure(EntityTypeBuilder<TimesheetSubmission> builder)
    {
        builder.ToTable("TimesheetSubmissions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("ts_sub_id");

        builder.Property(s => s.EmployeeId)
            .HasColumnName("ts_sub_employee_id")
            .IsRequired();

        builder.Property(s => s.Year)
            .HasColumnName("ts_sub_year")
            .IsRequired();

        builder.Property(s => s.Month)
            .HasColumnName("ts_sub_month")
            .IsRequired();

        builder.Property(s => s.SubmittedDate)
            .HasColumnName("ts_sub_submitted_date")
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("ts_sub_status")
            .IsRequired();

        builder.Property(s => s.RevisionNote)
            .HasColumnName("ts_sub_revision_note")
            .HasMaxLength(1000);

        builder.Property(s => s.ReviewedDate)
            .HasColumnName("ts_sub_reviewed_date");

        builder.Property(s => s.ReviewedBy)
            .HasColumnName("ts_sub_reviewed_by");

        builder.Property(s => s.IsDeleted).HasColumnName("ts_sub_is_deleted");
        builder.Property(s => s.CreatedBy).HasColumnName("ts_sub_created_by");
        builder.Property(s => s.CreatedUtcDate).HasColumnName("ts_sub_created_date");
        builder.Property(s => s.ModifiedBy).HasColumnName("ts_sub_modified_by");
        builder.Property(s => s.ModifiedUtcDate).HasColumnName("ts_sub_modified_date");

        // Logical relationships for high-scale enterprise infrastructure.
        // No physical database locks from Foreign Key constraints.
        
        // One active submission per employee per month
        builder.HasIndex(s => new { s.EmployeeId, s.Year, s.Month }).IsUnique();
    }
}
