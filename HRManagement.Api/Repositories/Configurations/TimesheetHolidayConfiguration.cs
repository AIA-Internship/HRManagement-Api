using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations
{
    public class TimesheetHolidayConfiguration : IEntityTypeConfiguration<TimesheetHoliday>
    {
        public void Configure(EntityTypeBuilder<TimesheetHoliday> builder)
        {
            builder.ToTable("TimesheetHolidays");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("ts_holiday_id");

            builder.Property(p => p.HolidayDate)
                .HasColumnName("ts_holiday_date")
                .IsRequired();

            builder.Property(p => p.Name)
                .HasColumnName("ts_holiday_name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasColumnName("ts_holiday_description")
                .HasMaxLength(500);

            builder.Property(p => p.IsDeleted).HasColumnName("ts_holiday_is_deleted");
            builder.Property(p => p.CreatedBy).HasColumnName("ts_holiday_created_by");
            builder.Property(p => p.CreatedUtcDate).HasColumnName("ts_holiday_created_date");
            builder.Property(p => p.ModifiedBy).HasColumnName("ts_holiday_modified_by");
            builder.Property(p => p.ModifiedUtcDate).HasColumnName("ts_holiday_modified_date");

            // Prevent duplicate holidays on the same date
            builder.HasIndex(p => p.HolidayDate).IsUnique();
        }
    }
}
