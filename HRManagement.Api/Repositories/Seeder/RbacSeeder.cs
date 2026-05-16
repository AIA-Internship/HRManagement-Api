using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Tables.MasterRole;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories.Seeder;

public static class RbacSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Seed Permissions
        if (!await context.Set<Permission>().AnyAsync())
        {
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
        }

        // 2. Check Roles (They should be seeded by migration, but we ensure they exist)
        var supervisorRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Supervisor");
        var employeeRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Employee");

        if (supervisorRole == null || employeeRole == null)
        {
            // This part should technically not be reached if migration seeded them, 
            // but kept for robustness.
            if (supervisorRole == null)
            {
                supervisorRole = new Role { Name = "Supervisor", Description = "Full access" };
                context.Set<Role>().Add(supervisorRole);
            }
            if (employeeRole == null)
            {
                employeeRole = new Role { Name = "Employee", Description = "Standard access" };
                context.Set<Role>().Add(employeeRole);
            }
            await context.SaveChangesAsync();
        }

        // 3. Map Permissions to Roles (If not already mapped)
        if (!await context.Set<RolePermission>().AnyAsync())
        {
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
}
