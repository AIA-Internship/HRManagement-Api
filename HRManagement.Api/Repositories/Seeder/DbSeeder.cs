using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Repositories.Base;

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
        // 2. CHECK IF EMPLOYEES ALREADY EXIST
        // ==========================================
        if (context.Employees.Any())
        {
            return; 
        }

        // ==========================================
        // 3. SEED THE ADMIN / SUPERVISOR
        // ==========================================
        var adminDto = new CreateEmployeeRequestDto
        {
            EmployeeEmail = "Brandon@aia.com",
            PersonalEmail = "brandon.personal@email.com",
            DefaultPassword = "AdminPass123!",
            FullName = "Brandon Admin",
            Gender = 0, // Male
            PhoneNumber = "081234567890",
            Nik = "HR-0001",
            PlaceOfBirth = "Jakarta",
            DateOfBirth = new DateTime(1998, 1, 15).ToUniversalTime(),
            MaritalStatus = 0, // Single
            StreetAddress = "Jl. Sudirman No. 1",
            City = "Jakarta",
            Province = "DKI Jakarta",
            PostalCode = "10220",
            Role = 0, // Supervisor
            EmploymentInformation = new CreateEmploymentInfoDto
            {
                EmploymentStatus = 1, // Active
                StartDate = DateTime.UtcNow,
                EmploymentType = 1, // Fulltime
                Department = "Human Resources",
                Position = "HR Manager",
                SupervisorName = "System Director",
                EmployeeDisplayId = "E150529"
            },
            EmergencyContacts = new List<CreateEmergencyContactDto>
            {
                new() { Name = "Jane Doe", Relationship = "Sister", PhoneNumber = "089876543210" }
            }
        };
        
        var adminEmployee = CreateEmployeeEntity(adminDto, 1);
        // var adminUser = new User(
        //     adminDto.EmployeeEmail, 
        //     passwordHasher.Hash(adminDto.DefaultPassword), 
        //     adminDto.Role, 
        //     1
        // );

        // ==========================================
        // 4. SEED THE INTERN
        // ==========================================
        var internDto = new CreateEmployeeRequestDto
        {
            EmployeeEmail = "Owen@aia.com",
            PersonalEmail = "owen.personal@email.com",
            DefaultPassword = "WorkerPass123!",
            FullName = "Owen Intern",
            Gender = 0, // Male
            PhoneNumber = "081298765432",
            Nik = "INT-0001",
            PlaceOfBirth = "Bandung",
            DateOfBirth = new DateTime(2002, 5, 20).ToUniversalTime(),
            MaritalStatus = 0, // Single
            StreetAddress = "Jl. Thamrin No. 10",
            City = "Jakarta",
            Province = "DKI Jakarta",
            PostalCode = "10350",
            Role = 1, // Employee (Intern role in User table might be different, but looking at lookups...)
            EmploymentInformation = new CreateEmploymentInfoDto
            {
                EmploymentStatus = 1, // Active
                StartDate = DateTime.UtcNow,
                EmploymentType = 3, // Intern
                Department = "Development",
                Position = "Software Engineering Intern",
                SupervisorName = "Brandon Admin",
                EmployeeDisplayId = "E150530"
            },
            EmergencyContacts = new List<CreateEmergencyContactDto>
            {
                new() { Name = "Sarah Intern", Relationship = "Mother", PhoneNumber = "087712345678" }
            }
        };
        
        var internEmployee = CreateEmployeeEntity(internDto, 1);
        // var internUser = new User(
        //     internDto.EmployeeEmail, 
        //     passwordHasher.Hash(internDto.DefaultPassword), 
        //     internDto.Role, 
        //     1
        // );

        // ==========================================
        // 5. SAVE TO DATABASE
        // ==========================================
        context.Employees.AddRange(adminEmployee, internEmployee);
        // context.Users.AddRange(adminUser, internUser);

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
            actionerId: actionerId,
            employmentInformation: employmentInformation,
            emergencyContacts: emergencyContacts);
    }
}
