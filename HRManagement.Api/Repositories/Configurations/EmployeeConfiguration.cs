using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasOne(e => e.EmploymentInformation)
            .WithOne(e => e.Employee)
            .HasForeignKey<EmploymentInformation>(e => e.EmployeeId);
        
        builder.HasMany(e => e.EmergencyContacts)
            .WithOne(ec => ec.Employee)
            .HasForeignKey(ec => ec.EmployeeId);
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("emp_id");
        
        builder.Property(e => e.FullName)
            .HasColumnName("emp_name")
            .HasMaxLength(150)
            .IsRequired();
        
        builder.Property(e => e.Gender)
            .HasColumnName("emp_gender")
            .IsRequired();
        
        builder.Property(e => e.PersonalEmail)
            .HasColumnName("emp_personal_email")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.EmployeeEmail)
            .HasColumnName("emp_work_email")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.Nik)
            .HasColumnName("emp_nik")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(e => e.PlaceOfBirth)
            .HasColumnName("emp_POB")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.DateOfBirth)
            .HasColumnName("emp_DOB")
            .HasColumnType("date")
            .IsRequired();
        
        builder.Property(e => e.MaritalStatus)
            .HasColumnName("emp_marital_status")
            .IsRequired();
        
        builder.OwnsOne(e => e.CurrentAddress, a =>
        {
            a.Property(p => p.Street).HasColumnName("emp_current_st_address").HasMaxLength(150).IsRequired();
            a.Property(p => p.City).HasColumnName("emp_current_city").HasMaxLength(100).IsRequired();
            a.Property(p => p.Province).HasColumnName("emp_current_province").HasMaxLength(50).IsRequired();
            a.Property(p => p.ZipCode).HasColumnName("emp_current_postal_code").HasMaxLength(15).IsRequired();
        });

        builder.OwnsOne(e => e.ResidentialAddress, a =>
        {
            a.Property(p => p.Street).HasColumnName("emp_residential_st_address").HasMaxLength(150).IsRequired();
            a.Property(p => p.City).HasColumnName("emp_residential_city").HasMaxLength(100).IsRequired();
            a.Property(p => p.Province).HasColumnName("emp_residential_province").HasMaxLength(50).IsRequired();
            a.Property(p => p.ZipCode).HasColumnName("emp_residential_postal_code").HasMaxLength(15).IsRequired();
        });

        builder.Property(e => e.PhoneNumber)
            .HasColumnName("emp_phone")
            .HasMaxLength(25)
            .IsRequired();
        
        builder.Property(e => e.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.HasOne(e => e.SystemRole)
            .WithMany(r => r.Employees)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.FullName).IsUnique();
        builder.HasIndex(e => e.PersonalEmail).IsUnique();
        builder.HasIndex(e => e.EmployeeEmail).IsUnique();
        builder.HasIndex(e => e.Nik).IsUnique();
        builder.HasIndex(e => e.PhoneNumber).IsUnique();
    }
}
