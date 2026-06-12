using HRManagement.Domain.Models.Tables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.MsSQL.Configurations;

public class UsersConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(c => c.Id);

        builder.HasIndex(u => u.EmployeeEmail).IsUnique();

        builder.HasOne(u => u.Role)
                   .WithMany(r => r.Users)
                   .HasForeignKey(u => u.RoleId)
                   .HasPrincipalKey(r => r.Id)
                   .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Actioner)
               .WithMany()
               .HasForeignKey(c => c.ModifiedBy)
               .OnDelete(DeleteBehavior.Restrict);

    }
    
}