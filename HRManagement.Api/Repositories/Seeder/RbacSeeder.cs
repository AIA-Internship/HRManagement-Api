using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Tables.MasterRole;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories.Seeder;

public static class RbacSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Set<Role>().AnyAsync()) return;

        // 1. Seed Permissions
        var permissions = new List<Permission>
        {
            new() { Name = Permissions.Employees.View },
            new() { Name = Permissions.Employees.Create },
            new() { Name = Permissions.Employees.Edit },
            new() { Name = Permissions.Employees.Delete },
            new() { Name = Permissions.Users.View },
            new() { Name = Permissions.Users.Edit }
        };

        context.Set<Permission>().AddRange(permissions);
        await context.SaveChangesAsync();

        // 2. Seed Roles
        var supervisorRole = new Role
        {
            Name = "Supervisor",
            Description = "Full access to employee management and reports"
        };

        var employeeRole = new Role
        {
            Name = "Employee",
            Description = "Standard access to personal info and limited features"
        };

        context.Set<Role>().AddRange(supervisorRole, employeeRole);
        await context.SaveChangesAsync();

        // 3. Map Permissions to Roles
        var allPermissions = await context.Set<Permission>().ToListAsync();
        
        // Supervisor gets everything
        foreach (var p in allPermissions)
        {
            context.Set<RolePermission>().Add(new RolePermission { RoleId = supervisorRole.Id, PermissionId = p.Id });
        }

        // Employee gets only View
        var viewPermissions = allPermissions.Where(p => p.Name.Contains("View")).ToList();
        foreach (var p in viewPermissions)
        {
            context.Set<RolePermission>().Add(new RolePermission { RoleId = employeeRole.Id, PermissionId = p.Id });
        }

        await context.SaveChangesAsync();
    }
}
