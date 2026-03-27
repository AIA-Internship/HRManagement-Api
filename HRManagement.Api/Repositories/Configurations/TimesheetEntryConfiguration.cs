using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

public class TimesheetEntryConfiguration : IEntityTypeConfiguration<TimesheetEntry>
{
    public void Configure(EntityTypeBuilder<TimesheetEntry> builder)
    {
        builder.ToTable("TimesheetEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("ts_entry_id");

        builder.Property(e => e.EmployeeId)
            .HasColumnName("ts_entry_employee_id")
            .IsRequired();

        builder.Property(e => e.EntryDate)
            .HasColumnName("ts_entry_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.DurationMinutes)
            .HasColumnName("ts_entry_duration_minutes")
            .IsRequired();

        builder.Property(e => e.ProjectId)
            .HasColumnName("ts_entry_project_id")
            .IsRequired();

        builder.Property(e => e.ApplicationUsed)
            .HasColumnName("ts_entry_app_used")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(e => e.TaskDescription)
            .HasColumnName("ts_entry_task_desc")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.ProjectLeadId)
            .HasColumnName("ts_entry_project_lead_id")
            .IsRequired();

        builder.Property(e => e.Location)
            .HasColumnName("ts_entry_location")
            .IsRequired();

        builder.Property(e => e.IsDeleted).HasColumnName("ts_entry_is_deleted");
        builder.Property(e => e.CreatedBy).HasColumnName("ts_entry_created_by");
        builder.Property(e => e.CreatedUtcDate).HasColumnName("ts_entry_created_date");
        builder.Property(e => e.ModifiedBy).HasColumnName("ts_entry_modified_by");
        builder.Property(e => e.ModifiedUtcDate).HasColumnName("ts_entry_modified_date");

        // Relationships are handled at application/logic level for Enterprise Scale.
        // No physical database Foreign Keys to simplify sharding and boost performance.
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.ProjectLeadId);
        
        // Composite index for fast per-employee per-date lookups
        builder.HasIndex(e => new { e.EmployeeId, e.EntryDate });
    }
}
