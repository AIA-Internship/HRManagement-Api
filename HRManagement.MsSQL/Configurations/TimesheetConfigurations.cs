using HRManagement.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class TimesheetSubmissionConfiguration : IEntityTypeConfiguration<TimesheetSubmission>
{
    public void Configure(EntityTypeBuilder<TimesheetSubmission> builder)
    {
        builder.ToTable("TimesheetSubmissions");
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.User);
        
        builder.Property(e => e.Id).HasColumnName("ts_sub_id");
        builder.Property(e => e.EmployeeId).HasColumnName("ts_sub_employee_id");
        builder.Property(e => e.Year).HasColumnName("ts_sub_year");
        builder.Property(e => e.Month).HasColumnName("ts_sub_month");
        builder.Property(e => e.SubmittedDate).HasColumnName("ts_sub_submitted_date");
        builder.Property(e => e.Status).HasColumnName("ts_sub_status");
        builder.Property(e => e.RevisionNote).HasColumnName("ts_sub_revision_note");
        builder.Property(e => e.ReviewedDate).HasColumnName("ts_sub_reviewed_date");
        builder.Property(e => e.ReviewedBy).HasColumnName("ts_sub_reviewed_by");

        builder.Property(e => e.IsDeleted).HasColumnName("ts_sub_is_deleted");
        builder.Property(e => e.CreatedBy).HasColumnName("ts_sub_created_by").HasConversion<long>();
        builder.Property(e => e.CreatedUtcDate).HasColumnName("ts_sub_created_date");
        builder.Property(e => e.ModifiedBy).HasColumnName("ts_sub_modified_by").HasConversion<long>();
        builder.Property(e => e.ModifiedUtcDate).HasColumnName("ts_sub_modified_date");
    }
}

public class TimesheetProjectConfiguration : IEntityTypeConfiguration<TimesheetProject>
{
    public void Configure(EntityTypeBuilder<TimesheetProject> builder)
    {
        builder.ToTable("TimesheetProjects");
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.User);

        builder.Property(e => e.Id).HasColumnName("ts_project_id");
        builder.Property(e => e.Name).HasColumnName("ts_project_name");
        builder.Property(e => e.Description).HasColumnName("ts_project_description");
        builder.Property(e => e.ProjectLeader).HasColumnName("ts_project_leader");
        builder.Property(e => e.Status).HasColumnName("ts_project_status");

        builder.Property(e => e.IsDeleted).HasColumnName("ts_project_is_deleted");
        builder.Property(e => e.CreatedBy).HasColumnName("ts_project_created_by").HasConversion<long>();
        builder.Property(e => e.CreatedUtcDate).HasColumnName("ts_project_created_date");
        builder.Property(e => e.ModifiedBy).HasColumnName("ts_project_modified_by").HasConversion<long>();
        builder.Property(e => e.ModifiedUtcDate).HasColumnName("ts_project_modified_date");
    }
}

public class TimesheetHolidayConfiguration : IEntityTypeConfiguration<TimesheetHoliday>
{
    public void Configure(EntityTypeBuilder<TimesheetHoliday> builder)
    {
        builder.ToTable("TimesheetHolidays");
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.User);

        builder.Property(e => e.Id).HasColumnName("ts_holiday_id");
        builder.Property(e => e.HolidayDate).HasColumnName("ts_holiday_date");
        builder.Property(e => e.Name).HasColumnName("ts_holiday_name");
        builder.Property(e => e.Description).HasColumnName("ts_holiday_description");

        builder.Property(e => e.IsDeleted).HasColumnName("ts_holiday_is_deleted");
        builder.Property(e => e.CreatedBy).HasColumnName("ts_holiday_created_by").HasConversion<long>();
        builder.Property(e => e.CreatedUtcDate).HasColumnName("ts_holiday_created_date");
        builder.Property(e => e.ModifiedBy).HasColumnName("ts_holiday_modified_by").HasConversion<long>();
        builder.Property(e => e.ModifiedUtcDate).HasColumnName("ts_holiday_modified_date");
    }
}

public class TimesheetEntryConfiguration : IEntityTypeConfiguration<TimesheetEntry>
{
    public void Configure(EntityTypeBuilder<TimesheetEntry> builder)
    {
        builder.ToTable("TimesheetEntries");
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.User);

        builder.Property(e => e.Id).HasColumnName("ts_entry_id");
        builder.Property(e => e.EmployeeId).HasColumnName("ts_entry_employee_id");
        builder.Property(e => e.EntryDate).HasColumnName("ts_entry_date");
        builder.Property(e => e.DurationMinutes).HasColumnName("ts_entry_duration_minutes");
        builder.Property(e => e.ProjectId).HasColumnName("ts_entry_project_id");
        builder.Property(e => e.ApplicationUsed).HasColumnName("ts_entry_app_used");
        builder.Property(e => e.TaskDescription).HasColumnName("ts_entry_task_desc");
        builder.Property(e => e.ProjectLeadId).HasColumnName("ts_entry_project_lead_id");
        builder.Property(e => e.Location).HasColumnName("ts_entry_location");
        
        builder.Ignore(e => e.DayType);

        builder.Property(e => e.IsDeleted).HasColumnName("ts_entry_is_deleted");
        builder.Property(e => e.CreatedBy).HasColumnName("ts_entry_created_by").HasConversion<long>();
        builder.Property(e => e.CreatedUtcDate).HasColumnName("ts_entry_created_date");
        builder.Property(e => e.ModifiedBy).HasColumnName("ts_entry_modified_by").HasConversion<long>();
        builder.Property(e => e.ModifiedUtcDate).HasColumnName("ts_entry_modified_date");
    }
}

public class TimesheetDayCommentConfiguration : IEntityTypeConfiguration<TimesheetDayComment>
{
    public void Configure(EntityTypeBuilder<TimesheetDayComment> builder)
    {
        builder.ToTable("TimesheetDayComments");
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.User);

        builder.Property(e => e.Id).HasColumnName("ts_comment_id");
        builder.Property(e => e.SubmissionId).HasColumnName("ts_comment_submission_id");
        builder.Property(e => e.CommentDate).HasColumnName("ts_comment_date");
        builder.Property(e => e.Comment).HasColumnName("ts_comment_text");

        builder.Property(e => e.IsDeleted).HasColumnName("ts_comment_is_deleted");
        builder.Property(e => e.CreatedBy).HasColumnName("ts_comment_created_by").HasConversion<long>();
        builder.Property(e => e.CreatedUtcDate).HasColumnName("ts_comment_created_date");
        builder.Property(e => e.ModifiedBy).HasColumnName("ts_comment_modified_by").HasConversion<long>();
        builder.Property(e => e.ModifiedUtcDate).HasColumnName("ts_comment_modified_date");
    }
}

public class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        builder.ToTable("TodoTasks");
        builder.HasKey(e => e.Id);
        builder.Ignore(e => e.User);

        builder.Property(e => e.Id).HasColumnName("todo_id");
        builder.Property(e => e.EmployeeId).HasColumnName("todo_employee_id");
        builder.Property(e => e.TaskName).HasColumnName("todo_task_name");
        builder.Property(e => e.DueDate).HasColumnName("todo_due_date");
        builder.Property(e => e.Priority).HasColumnName("todo_priority");
        builder.Property(e => e.IsCompleted).HasColumnName("todo_is_completed");

        builder.Property(e => e.IsDeleted).HasColumnName("todo_is_deleted");
        builder.Property(e => e.CreatedBy).HasColumnName("todo_created_by").HasConversion<long>();
        builder.Property(e => e.CreatedUtcDate).HasColumnName("todo_created_date");
        builder.Property(e => e.ModifiedBy).HasColumnName("todo_modified_by").HasConversion<long>();
        builder.Property(e => e.ModifiedUtcDate).HasColumnName("todo_modified_date");
    }
}
