using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class EmployeeUpdateRequestConfiguration : IEntityTypeConfiguration<EmployeeUpdateRequest>
{
    public void Configure(EntityTypeBuilder<EmployeeUpdateRequest> builder)
    {
        builder.ToTable("EmployeeUpdateRequest");
        builder.HasKey(e => e.Id);

        // Property names differ from physical DB columns — map explicitly
        builder.Property(e => e.NewPlaceOfBirth).HasColumnName("NewBirthPlace");
        builder.Property(e => e.NewDateOfBirth).HasColumnName("NewBirthDate");
        builder.Property(e => e.NewPhoneNumber).HasColumnName("NewMobilePhone");
        builder.Property(e => e.NewCurrentStreetAddress).HasColumnName("NewCurrentAddress");
        builder.Property(e => e.NewCurrentZipCode).HasColumnName("NewCurrentPostalCode");
        builder.Property(e => e.NewResidentialStreetAddress).HasColumnName("NewResidentialAddress");
        builder.Property(e => e.NewResidentialZipCode).HasColumnName("NewResidentialPostalCode");
        builder.Property(e => e.Status).HasColumnName("RequestStatus");

        builder.HasOne(e => e.Employee)
            .WithMany() 
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);
    }
}