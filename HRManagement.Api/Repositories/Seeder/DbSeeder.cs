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
        // 2. SEED DEFAULT USERS (IF NOT EXIST)
        // ==========================================
        var usersToSeed = new List<(string Email, string Password, int Role, string FullName, string DisplayId)>
        {
            ("Brandon@aia.com", "AdminPass123!", 0, "Brandon Admin", "E150529"),
            ("Owen@aia.com", "WorkerPass123!", 1, "Owen Intern", "E150530")
        };

        foreach (var u in usersToSeed)
        {
            if (!await context.Users.AnyAsync(user => user.EmployeeEmail == u.Email))
            {
                // Create minimal user and employee record
                var user = new User(u.Email, passwordHasher.Hash(u.Password), u.Role, 1);
                context.Users.Add(user);
                
                // Add minimal employee record if missing
                if (!await context.Employees.AnyAsync(e => e.EmployeeEmail == u.Email))
                {
                    var emp = new Employee(
                        fullName: u.FullName,
                        gender: 0,
                        personalEmail: u.Email.Replace("@aia.com", "@personal.com"),
                        employeeEmail: u.Email,
                        phoneNumber: "08123456789",
                        nik: u.DisplayId,
                        placeOfBirth: "Jakarta",
                        dateOfBirth: DateTime.UtcNow.AddYears(-25),
                        maritalStatus: 0,
                        streetAddress: "Jl. Sudirman",
                        city: "Jakarta",
                        province: "DKI Jakarta",
                        postalCode: "12345",
                        role: u.Role,
                        actionerId: 1);
                    
                    context.Employees.Add(emp);
                    await context.SaveChangesAsync(); // Save to get emp_id

                    var info = new EmploymentInformation(1)
                    {
                        EmployeeId = emp.Id,
                        EmployeeDisplayId = u.DisplayId,
                        Position = u.Role == 0 ? "Manager" : "Intern",
                        Department = "IT",
                        StartDate = DateTime.UtcNow,
                        SupervisorName = u.Role == 1 ? "Brandon Admin" : ""
                    };
                    context.EmploymentInformation.Add(info);
                }
            }
        }

        await context.SaveChangesAsync();

    }

    private static Employee CreateEmployeeEntity(CreateEmployeeRequestDto dto, long actionerId)
    {
        var employmentInformation = dto.EmploymentInformation == null
            ? null
            : new EmploymentInformation(actionerId)
            {
                EmploymentStatus = dto.EmploymentInformation.EmploymentStatus,
                StartDate = dto.EmploymentInformation.StartDate,
                EmploymentType = dto.EmploymentInformation.EmploymentType,
                Department = dto.EmploymentInformation.Department,
                Position = dto.EmploymentInformation.Position,
                SupervisorName = dto.EmploymentInformation.SupervisorName,
                EmployeeDisplayId = dto.EmploymentInformation.EmployeeDisplayId
            };

        var emergencyContacts = dto.EmergencyContacts
            .Select(x => new EmergencyContact
            {
                Name = x.Name,
                Relationship = x.Relationship,
                PhoneNumber = x.PhoneNumber
            })
            .ToList(); 

        return new Employee(
            fullName: dto.FullName,
            gender: dto.Gender,
            personalEmail: dto.PersonalEmail,
            employeeEmail: dto.EmployeeEmail,
            phoneNumber: dto.PhoneNumber,
            nik: dto.Nik,
            placeOfBirth: dto.PlaceOfBirth,
            dateOfBirth: dto.DateOfBirth,
            maritalStatus: dto.MaritalStatus,
            streetAddress: dto.StreetAddress,
            city: dto.City,
            province: dto.Province,
            postalCode: dto.PostalCode,
            role: dto.Role,
            actionerId: actionerId);
    }
}
