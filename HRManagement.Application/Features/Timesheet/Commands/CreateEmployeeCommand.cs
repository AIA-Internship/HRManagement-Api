using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Commands.Dto;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using System.Text.RegularExpressions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Commands;

public class CreateEmployeeCommand(CreateEmployeeRequestDto commandDto) : IRequest<ApiResponse<string>>
{
    public CreateEmployeeRequestDto RequestDto { get; } = commandDto;
    
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService, IPasswordHasher passwordHasher) : IRequestHandler<CreateEmployeeCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(CreateEmployeeCommand command, CancellationToken cancellationToken)
        {
            var actionerId = currentUserService.UserId;
            var dto = command.RequestDto;

            EmploymentInformation? empInfo = null;
            if (dto.EmploymentInformation != null)
            {
                var displayId = dto.EmploymentInformation.EmployeeDisplayId;
                if (string.IsNullOrWhiteSpace(displayId))
                {
                    displayId = await GenerateNextDisplayId(employeeRepository);
                }

                string? supervisorName = null;
                if (!string.IsNullOrWhiteSpace(dto.EmploymentInformation.SupervisorDisplayId))
                {
                    var supervisor = await employeeRepository.GetProfileByDisplayIdAsync(dto.EmploymentInformation.SupervisorDisplayId, cancellationToken);
                    supervisorName = supervisor?.FullName;
                }

                empInfo = new EmploymentInformation(
                    employeeId: 0,
                    statusCode: dto.EmploymentInformation.EmploymentStatus,
                    startDate: dto.EmploymentInformation.StartDate,
                    type: dto.EmploymentInformation.EmploymentType,
                    department: dto.EmploymentInformation.Department,
                    position: dto.EmploymentInformation.Position,
                    displayId: displayId,
                    supervisorId: null,
                    supervisorName: supervisorName,
                    actionerId: actionerId
                );
            }

            var contacts = new List<EmergencyContact>();
            if (dto.EmergencyContact != null && dto.EmergencyContact.Any())
            {
                foreach (var contactDto in dto.EmergencyContact)
                {
                    contacts.Add(new EmergencyContact(0, contactDto.Name, contactDto.PhoneNumber, contactDto.Relationship, actionerId));
                }
            }

            var currentAddress = new Address(dto.CurrentStreetAddress, dto.CurrentCity, dto.CurrentProvince, dto.CurrentPostalCode);
            var residentialAddress = new Address(dto.ResidentialStreetAddress, dto.ResidentialCity, dto.ResidentialProvince, dto.ResidentialPostalCode);

            var employee = new Employee(
                fullName: dto.FullName,
                gender: dto.Gender.ToString(),
                personalEmail: dto.PersonalEmail,
                employeeEmail: dto.EmployeeEmail,
                mobilePhone: dto.PhoneNumber,
                nik: dto.Nik,
                placeOfBirth: dto.PlaceOfBirth,
                dateOfBirth: dto.DateOfBirth,
                maritalStatus: dto.MaritalStatus,
                currentAddress: currentAddress,
                residentialAddress: residentialAddress,
                roleId: dto.Role,
                actionerId: actionerId,
                employmentInformation: empInfo,
                emergencyContact: contacts.FirstOrDefault()
            );

            var hashedPassword = passwordHasher.Hash(dto.DefaultPassword);
            var user = new Users(dto.EmployeeEmail, hashedPassword, dto.Role, actionerId);
            
            var userProp = typeof(Employee).GetProperty("User");
            if (userProp != null && userProp.CanWrite) userProp.SetValue(employee, user);
            else {
                var field = typeof(Employee).GetField("<User>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) field.SetValue(employee, user);
            }
            
            await employeeRepository.AddAsync(employee, cancellationToken);

            return ApiHelperResponse.Success<string>("Employee and Users Account created successfully", "Success");
        }

        private static async Task<string> GenerateNextDisplayId(IEmployeeRepository repository)
        {
            var lastId = await repository.GetLastEmployeeDisplayIdAsync();
            if (string.IsNullOrEmpty(lastId))
            {
                return "E0001";
            }

            var match = Regex.Match(lastId, @"(\D*)(\d+)");
            if (match.Success)
            {
                var prefix = match.Groups[1].Value;
                var numberStr = match.Groups[2].Value;

                if (int.TryParse(numberStr, out var number))
                {
                    var nextNumber = number + 1;
                    if (string.IsNullOrEmpty(prefix)) prefix = "E";

                    return $"{prefix}{nextNumber.ToString().PadLeft(numberStr.Length, '0')}";
                }
            }

            return "E0001";
        }
    }
}
