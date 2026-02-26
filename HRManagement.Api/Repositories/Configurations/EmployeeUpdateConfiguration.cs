using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

public class EmployeeUpdateRequestConfiguration : IEntityTypeConfiguration<EmployeeUpdateRequest>
{
    public void Configure(EntityTypeBuilder<EmployeeUpdateRequest> builder)
    {
        builder.ToTable("EmployeeUpdateRequests");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("request_id");
        
        builder.Property(e => e.EmployeeId)
            .HasColumnName("emp_id")
            .IsRequired(); 
        
        builder.Property(e => e.NewFullName)
            .HasColumnName("new_full_name")
            .HasMaxLength(150)
            .IsRequired(false);
        
        builder.Property(e => e.NewGender)
            .HasColumnName("new_gender")
            .IsRequired(false);
        
        builder.Property(e => e.NewPersonalEmail)
            .HasColumnName("new_personal_email")
            .HasMaxLength(150)
            .IsRequired(false);
        
        builder.Property(e => e.NewPlaceOfBirth)
            .HasColumnName("new_place_of_birth")
            .HasMaxLength(150)
            .IsRequired(false);
        
        builder.Property(e => e.NewDateOfBirth)
            .HasColumnName("new_date_of_birth")
            .HasColumnType("date")
            .IsRequired(false);
        
        builder.Property(e => e.NewMaritalStatus)
            .HasColumnName("new_marital_status")
            .IsRequired(false);

        builder.Property(e => e.NewStreetAddress)
            .HasColumnName("new_street_address")
            .HasMaxLength(150)
            .IsRequired(false);
        
        builder.Property(e => e.NewCity)
            .HasColumnName("new_city")
            .HasMaxLength(100)
            .IsRequired(false);
        
        builder.Property(e => e.NewProvince)
            .HasColumnName("new_province")
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.Property(e => e.NewPostalCode)
            .HasColumnName("new_postal_code")
            .HasMaxLength(15)
            .IsRequired(false);

        builder.Property(e => e.NewPhoneNumber)
            .HasColumnName("new_phone_number")
            .HasMaxLength(25)
            .IsRequired(false);
        
        builder.Property(e => e.Status)
            .HasColumnName("request_status")
            .HasConversion<string>() 
            .IsRequired();

        builder.Property(e => e.HrReason)
            .HasColumnName("hr_reason")
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.HasOne(e => e.Employee)
            .WithMany() 
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}