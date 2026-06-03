using HRManagement.Api.Domain.Models.Tables.MasterRole;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRManagement.Api.Repositories.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("MasterRolePermissions");
        
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
    }
}
