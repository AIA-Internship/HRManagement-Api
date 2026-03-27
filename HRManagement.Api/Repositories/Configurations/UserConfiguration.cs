using HRManagement.Api.Domain.Models.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("user_id");
        
        builder.Property(e => e.EmployeeEmail)
            .HasColumnName("employee_email")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(e => e.Role)
            .HasColumnName("user_role")
            .IsRequired();
        
        builder.HasIndex(e => e.EmployeeEmail).IsUnique();
    }
    
}