using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using System.Text.RegularExpressions;
using MediatR;

namespace HRManagement.Api.Application.Commands;

public class CreateEmployeeCommand(CreateEmployeeRequestDto commandDto) : IRequest<ApiResponse<string>>
{
    public CreateEmployeeRequestDto RequestDto { get; } = commandDto;
    
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService, IPasswordHasher passwordHasher) : IRequestHandler<CreateEmployeeCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(CreateEmployeeCommand command, CancellationToken cancellationToken)
        {
            var actionerId = currentUserService.UserId;
            var dto = command.RequestDto;
            
            EmploymentInformation? employmentInfo = null;
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
                    var supervisor = await employeeRepository.GetByDisplayIdAsync(dto.EmploymentInformation.SupervisorDisplayId);
                    supervisorName = supervisor?.FullName;
                }

                employmentInfo = new EmploymentInformation(actionerId);
                employmentInfo.UpdateDetails(
                    dto.EmploymentInformation.EmploymentStatus,
                    dto.EmploymentInformation.StartDate,
                    dto.EmploymentInformation.EmploymentType,
                    dto.EmploymentInformation.Department,
                    dto.EmploymentInformation.Position,
                    supervisorName,
                    displayId,
                    actionerId
                );
            }
            
            var emergencyContacts = new List<EmergencyContact>();
            if (dto.EmergencyContacts != null && dto.EmergencyContacts.Any())
            {
                foreach (var contactDto in dto.EmergencyContacts)
                {
                    emergencyContacts.Add(new EmergencyContact
                    {
                        Name = contactDto.Name,
                        Relationship = contactDto.Relationship,
                        PhoneNumber = contactDto.PhoneNumber,
                        CreatedBy = actionerId,
                        ModifiedBy = actionerId
                    });
                }
            }
            
            var employee = new Employee(
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
                role: dto.Role,
                actionerId: actionerId);
            
            var hashedPassword = passwordHasher.Hash(dto.DefaultPassword);
            var user = new User(dto.EmployeeEmail, hashedPassword, dto.Role, actionerId);
            
            await employeeRepository.AddEmployeeAsync(user, employee, employmentInfo, emergencyContacts);

            return ApiHelperResponse.Success("Employee and User Account created successfully", "Success");
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

                if (long.TryParse(numberStr, out var number))
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