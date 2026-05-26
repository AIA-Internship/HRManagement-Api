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
        
        builder.Property(e => e.NewNik)
            .HasColumnName("new_nik")
            .HasMaxLength(255)
            .IsRequired(false);
        
        builder.Property(e => e.NewDateOfBirth)
            .HasColumnName("new_date_of_birth")
            .HasColumnType("date")
            .IsRequired(false);
        
        builder.Property(e => e.NewMaritalStatus)
            .HasColumnName("new_marital_status")
            .IsRequired(false);

        builder.Property(e => e.NewCurrentStreetAddress)
            .HasColumnName("new_current_street_address")
            .HasMaxLength(150)
            .IsRequired(false);
        
        builder.Property(e => e.NewCurrentCity)
            .HasColumnName("new_current_city")
            .HasMaxLength(100)
            .IsRequired(false);
        
        builder.Property(e => e.NewCurrentProvince)
            .HasColumnName("new_current_province")
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.Property(e => e.NewCurrentZipCode)
            .HasColumnName("new_current_postal_code")
            .HasMaxLength(15)
            .IsRequired(false);

        builder.Property(e => e.NewResidentialStreetAddress)
            .HasColumnName("new_residential_street_address")
            .HasMaxLength(150)
            .IsRequired(false);
        
        builder.Property(e => e.NewResidentialCity)
            .HasColumnName("new_residential_city")
            .HasMaxLength(100)
            .IsRequired(false);
        
        builder.Property(e => e.NewResidentialProvince)
            .HasColumnName("new_residential_province")
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.Property(e => e.NewResidentialZipCode)
            .HasColumnName("new_residential_postal_code")
            .HasMaxLength(15)
            .IsRequired(false);

        builder.Property(e => e.NewPhoneNumber)
            .HasColumnName("new_phone_number")
            .HasMaxLength(25)
            .IsRequired(false);
        
        builder.Property(e => e.Status)
            .HasColumnName("request_status")
            .IsRequired();

        builder.Property(e => e.HrReason)
            .HasColumnName("hr_reason")
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .HasColumnName("created_at")
            .IsRequired();
        
        builder.HasIndex(e => e.EmployeeId);
        
        builder.HasOne(e => e.Employee)
            .WithMany() 
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}