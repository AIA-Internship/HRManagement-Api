using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

public class TimesheetProjectConfiguration : IEntityTypeConfiguration<TimesheetProject>
{
    public void Configure(EntityTypeBuilder<TimesheetProject> builder)
    {
        builder.ToTable("TimesheetProjects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("ts_project_id");

        builder.Property(p => p.Name)
            .HasColumnName("ts_project_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("ts_project_description")
            .HasMaxLength(500);

        builder.Property(p => p.ProjectLeader)
            .HasColumnName("ts_project_leader")
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(p => p.Status)
            .HasColumnName("ts_project_status")
            .IsRequired();

        builder.Property(p => p.IsDeleted).HasColumnName("ts_project_is_deleted");
        builder.Property(p => p.CreatedBy).HasColumnName("ts_project_created_by");
        builder.Property(p => p.CreatedUtcDate).HasColumnName("ts_project_created_date");
        builder.Property(p => p.ModifiedBy).HasColumnName("ts_project_modified_by");
        builder.Property(p => p.ModifiedUtcDate).HasColumnName("ts_project_modified_date");

        builder.HasIndex(p => p.Name).IsUnique();
    }
}
