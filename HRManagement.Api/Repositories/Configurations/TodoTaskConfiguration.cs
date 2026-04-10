using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

public class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        builder.ToTable("TodoTasks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("todo_id");

        builder.Property(t => t.EmployeeId)
            .HasColumnName("todo_employee_id")
            .IsRequired();

        builder.Property(t => t.TaskName)
            .HasColumnName("todo_task_name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(t => t.DueDate)
            .HasColumnName("todo_due_date")
            .HasColumnType("date");

        builder.Property(t => t.Priority)
            .HasColumnName("todo_priority")
            .IsRequired();

        builder.Property(t => t.IsCompleted)
            .HasColumnName("todo_is_completed");

        builder.Property(t => t.IsDeleted).HasColumnName("todo_is_deleted");
        builder.Property(t => t.CreatedBy).HasColumnName("todo_created_by");
        builder.Property(t => t.CreatedUtcDate).HasColumnName("todo_created_date");
        builder.Property(t => t.ModifiedBy).HasColumnName("todo_modified_by");
        builder.Property(t => t.ModifiedUtcDate).HasColumnName("todo_modified_date");

        // Decoupled Relationship: Removing DB-level FK constraint for Enterprise Scale.
        // Linkage is managed at the application level using EmployeeId.
        builder.HasIndex(t => new { t.EmployeeId, t.IsDeleted });
    }
}
