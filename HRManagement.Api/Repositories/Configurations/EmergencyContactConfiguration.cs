using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

public class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.ToTable("EmergencyContacts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("emergency_contact_id");
        
        builder.Property(e => e.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();
        
        builder.Property(e => e.Name)
            .HasColumnName("emergency_contact_name")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.PhoneNumber)
            .HasColumnName("emergency_contact_phone")
            .HasMaxLength(25)
            .IsRequired();
        
        builder.Property(e => e.Relationship) 
            .HasColumnName("emergency_contact_relationship")
            .HasMaxLength(25)
            .IsRequired();
        
        // Database isolation for enterprise-scale: Removing DB-level FK constraint.
        // Relationship is managed at the application/logic level (Id-based).
        builder.HasIndex(e => e.EmployeeId);
    }
}