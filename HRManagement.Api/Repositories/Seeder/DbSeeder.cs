using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.Models.Tables.MasterRole;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories.Seeder;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        // ==========================================
        // 0. ENSURE SCHEMA IS UPDATED (Migration Guard)
        // ==========================================
        await EnsureSchemaUpdatedAsync(context);

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

        var roles = await context.Set<Role>().ToListAsync();
        var supervisorRoleId = roles.FirstOrDefault(r => r.Name == "Supervisor")?.Id ?? 0;
        var employeeRoleId = roles.FirstOrDefault(r => r.Name == "Employee")?.Id ?? 1;

        // ==========================================
        // 2. SEED DEFAULT USERS (Enforce Password Reset if Needed)
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
            CurrentStreetAddress = "Jl. Sudirman No. 1",
            CurrentCity = "Jakarta",
            CurrentProvince = "DKI Jakarta",
            CurrentPostalCode = "10220",
            ResidentialStreetAddress = "Jl. Sudirman No. 1",
            ResidentialCity = "Jakarta",
            ResidentialProvince = "DKI Jakarta",
            ResidentialPostalCode = "10220",
            Role = supervisorRoleId,
            EmploymentInformation = new CreateEmploymentInfoDto
            {
                EmploymentStatus = 1, // Active
                StartDate = DateTime.UtcNow,
                EmploymentType = 1, // Fulltime
                Department = "Human Resources",
                Position = "HR Manager",
                SupervisorDisplayId = null,
                EmployeeDisplayId = "E0001"
            },
            EmergencyContacts = new List<CreateEmergencyContactDto>
            {
                new() { Name = "Jane Doe", Relationship = "Sister", PhoneNumber = "089876543210" }
            }
        };

        // ==========================================
        // 2. CHECK IF EVERYTHING IS ALREADY SEEDED
        // ==========================================
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.EmployeeEmail == adminDto.EmployeeEmail);
        if (existingUser != null)
        {
            bool needsUpdate = false;
            
            // Update RoleId if it doesn't match the current database IDs
            if (existingUser.RoleId != supervisorRoleId && supervisorRoleId != 0)
            {
                existingUser.ChangeRole(supervisorRoleId, 1);
                needsUpdate = true;
            }

            // Ensure password matches the seeded value
            if (!passwordHasher.Verify(adminDto.DefaultPassword, existingUser.PasswordHash))
            {
                var expectedHash = passwordHasher.Hash(adminDto.DefaultPassword);
                existingUser.ChangePassword(expectedHash, 1);
                needsUpdate = true;
            }
            
            if (needsUpdate)
            {
                await context.SaveChangesAsync();
            }
            return; 
        }
        else
        {
            // Admin user NOT found
        }

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
            CurrentStreetAddress = "Jl. Thamrin No. 10",
            CurrentCity = "Jakarta",
            CurrentProvince = "DKI Jakarta",
            CurrentPostalCode = "10350",
            ResidentialStreetAddress = "Jl. Thamrin No. 10",
            ResidentialCity = "Jakarta",
            ResidentialProvince = "DKI Jakarta",
            ResidentialPostalCode = "10350",
            Role = employeeRoleId,
            EmploymentInformation = new CreateEmploymentInfoDto
            {
                EmploymentStatus = 1, // Active
                StartDate = DateTime.UtcNow,
                EmploymentType = 3, // Intern
                Department = "Development",
                Position = "Software Engineering Intern",
                SupervisorDisplayId = null,
                EmployeeDisplayId = "E0002"
            },
            EmergencyContacts = new List<CreateEmergencyContactDto>
            {
                new() { Name = "Sarah Intern", Relationship = "Mother", PhoneNumber = "087712345678" }
            }
        };

        // ==========================================
        // 3. SEED EMPLOYEES (If missing)
        // ==========================================
        var adminEmployee = CreateEmployeeEntity(adminDto, 1);
        var internEmployee = CreateEmployeeEntity(internDto, 1);

        if (internEmployee.EmploymentInformation != null)
        {
            internEmployee.EmploymentInformation.Supervisor = adminEmployee;
        }

        context.Employees.AddRange(adminEmployee, internEmployee);

        // ==========================================
        // 4. SEED USERS (If missing)
        // ==========================================
        var adminUser = new User(
            email: adminDto.EmployeeEmail, 
            passwordHash: passwordHasher.Hash(adminDto.DefaultPassword), 
            roleId: adminDto.Role, 
            actionerId: 1);
            
        var internUser = new User(
            email: internDto.EmployeeEmail, 
            passwordHash: passwordHasher.Hash(internDto.DefaultPassword), 
            roleId: internDto.Role, 
            actionerId: 1);

        context.Users.AddRange(adminUser, internUser);

        await context.SaveChangesAsync();
    }

    private static async Task EnsureSchemaUpdatedAsync(AppDbContext context)
    {
        // This helper method is no longer using the Azure SQL raw fallback because migrations are used.
        await Task.CompletedTask;
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
                SupervisorId = null, 
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
            currentAddress: new Address(dto.CurrentStreetAddress, dto.CurrentCity, dto.CurrentProvince, dto.CurrentPostalCode),
            residentialAddress: new Address(dto.ResidentialStreetAddress, dto.ResidentialCity, dto.ResidentialProvince, dto.ResidentialPostalCode),
            roleId: dto.Role,
            actionerId: actionerId,
            employmentInformation: employmentInformation,
            emergencyContacts: emergencyContacts);
    }
}
