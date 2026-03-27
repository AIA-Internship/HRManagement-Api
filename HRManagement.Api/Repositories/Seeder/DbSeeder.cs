using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories.Seeder;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        // ==========================================
        // 1. SEED SYSTEM LOOKUPS (Dropdown Data)
        // ==========================================
        if (!context.SystemLookups.Any())
        {
            var lookups = new List<SystemLookup>
            {
                // GenderStatus
                new SystemLookup { Category = "GENDER", Value = 0, DisplayName = "Male", IsActive = true },
                new SystemLookup { Category = "GENDER", Value = 1, DisplayName = "Female", IsActive = true },
                
                // MaritalStatus
                new SystemLookup { Category = "MARITAL_STATUS", Value = 0, DisplayName = "Single", IsActive = true },
                new SystemLookup { Category = "MARITAL_STATUS", Value = 1, DisplayName = "Married", IsActive = true },
                
                // UserRole
                new SystemLookup { Category = "ROLE", Value = 0, DisplayName = "Supervisor", IsActive = true },
                new SystemLookup { Category = "ROLE", Value = 1, DisplayName = "Employee", IsActive = true },
                
                // EmployeeStatus
                new SystemLookup { Category = "EMPLOYMENT_STATUS", Value = 0, DisplayName = "Inactive", IsActive = true },
                new SystemLookup { Category = "EMPLOYMENT_STATUS", Value = 1, DisplayName = "Active", IsActive = true },

                // EmploymentType
                new SystemLookup { Category = "EMPLOYMENT_TYPE", Value = 0, DisplayName = "Unknown", IsActive = true },
                new SystemLookup { Category = "EMPLOYMENT_TYPE", Value = 1, DisplayName = "Full-Time", IsActive = true },
                new SystemLookup { Category = "EMPLOYMENT_TYPE", Value = 2, DisplayName = "Part-Time", IsActive = true },
                new SystemLookup { Category = "EMPLOYMENT_TYPE", Value = 3, DisplayName = "Intern", IsActive = true },
                new SystemLookup { Category = "EMPLOYMENT_TYPE", Value = 4, DisplayName = "Contract", IsActive = true },
                
                // Request Status
                new SystemLookup { Category = "REQUEST_STATUS", Value = 0, DisplayName = "Pending", IsActive = true },
                new SystemLookup { Category = "REQUEST_STATUS", Value = 1, DisplayName = "Approved", IsActive = true },
                new SystemLookup { Category = "REQUEST_STATUS", Value = 2, DisplayName = "Rejected", IsActive = true },
            };

            context.SystemLookups.AddRange(lookups);
            await context.SaveChangesAsync();
        }
        
        // ==========================================
        // 2. CHECK IF EVERYTHING IS ALREADY SEEDED
        // ==========================================
        if (context.Employees.Any() && context.Users.Any())
        {
            return; 
        }

        // ==========================================
        // 3. SEED DEFAULT USERS (IF NOT EXIST)
        // ==========================================
        var adminsToSeed = new List<(string Email, string Password, string FullName, string DisplayId)>
        {
            ("Brandon@aia.com", "AdminPass123!", "Brandon Admin", "E0001")
        };

        var internsToSeed = new List<(string Email, string Password, string FullName, string DisplayId, string SupervisorName)>
        {
            ("Owen@aia.com", "WorkerPass123!", "Owen Intern", "E0002", "Brandon Admin")
        };

        // Seed Admins
        foreach (var u in adminsToSeed)
        {
            if (!await context.Users.AnyAsync(user => user.EmployeeEmail == u.Email))
            {
                var user = new User(u.Email, passwordHasher.Hash(u.Password), 0, 1);
                context.Users.Add(user);
                
                var emp = new Employee(
                    u.FullName, 0, u.Email.Replace("@aia.com", "@personal.com"), u.Email, "081234567890", u.DisplayId, "Jakarta", 
                    new DateTime(1998, 1, 15), 0, new Address("Jl. Sudirman No. 1", "Jakarta", "DKI Jakarta", "10220"), 
                    new Address("Jl. Sudirman No. 1", "Jakarta", "DKI Jakarta", "10220"), 0, 1);
                
                context.Employees.Add(emp);
                await context.SaveChangesAsync(); 

                var info = new EmploymentInformation(1)
                {
                    EmployeeId = emp.Id,
                    EmployeeDisplayId = u.DisplayId,
                    Position = "HR Manager",
                    Department = "Human Resources",
                    StartDate = DateTime.UtcNow,
                    EmploymentStatus = 1,
                    EmploymentType = 1,
                    SupervisorName = string.Empty
                };
                context.EmploymentInformation.Add(info);
                
                var contact = new EmergencyContact { EmployeeId = emp.Id, Name = "Jane Doe", Relationship = "Sister", PhoneNumber = "089876543210" };
                context.EmergencyContacts.Add(contact);
            }
        }

        // Seed Interns
        foreach (var u in internsToSeed)
        {
            if (!await context.Users.AnyAsync(user => user.EmployeeEmail == u.Email))
            {
                var user = new User(u.Email, passwordHasher.Hash(u.Password), 1, 1);
                context.Users.Add(user);
                
                var emp = new Employee(
                    u.FullName, 0, u.Email.Replace("@aia.com", "@personal.com"), u.Email, "081298765432", u.DisplayId, "Bandung", 
                    new DateTime(2002, 5, 20), 0, new Address("Jl. Thamrin No. 10", "Jakarta", "DKI Jakarta", "10350"), 
                    new Address("Jl. Thamrin No. 10", "Jakarta", "DKI Jakarta", "10350"), 1, 1);
                
                context.Employees.Add(emp);
                await context.SaveChangesAsync(); 

                var info = new EmploymentInformation(1)
                {
                    EmployeeId = emp.Id,
                    EmployeeDisplayId = u.DisplayId,
                    Position = "Software Engineering Intern",
                    Department = "Development",
                    StartDate = DateTime.UtcNow,
                    EmploymentStatus = 1,
                    EmploymentType = 3,
                    SupervisorName = u.SupervisorName
                };
                context.EmploymentInformation.Add(info);
                
                var contact = new EmergencyContact { EmployeeId = emp.Id, Name = "Sarah Intern", Relationship = "Mother", PhoneNumber = "087712345678" };
                context.EmergencyContacts.Add(contact);
            }
        }

        await context.SaveChangesAsync();
    }
}
