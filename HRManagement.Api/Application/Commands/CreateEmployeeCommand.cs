using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
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
                employmentInfo = new EmploymentInformation(actionerId);
                employmentInfo.UpdateDetails(
                    dto.EmploymentInformation.EmploymentStatus,
                    dto.EmploymentInformation.StartDate,
                    dto.EmploymentInformation.EmploymentType,
                    dto.EmploymentInformation.Department,
                    dto.EmploymentInformation.Position,
                    dto.EmploymentInformation.SupervisorName,
                    dto.EmploymentInformation.EmployeeDisplayId,
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
                streetAddress: dto.StreetAddress,
                city: dto.City,
                province: dto.Province,
                postalCode: dto.PostalCode,
                role: dto.Role,
                actionerId: actionerId,
                employmentInformation: employmentInfo,
                emergencyContacts: emergencyContacts
            );
            
            var hashedPassword = passwordHasher.Hash(dto.DefaultPassword);
            var user = new User(dto.EmployeeEmail, hashedPassword, dto.Role, actionerId);
            
            await employeeRepository.AddEmployeeAsync(user, employee);

            return ApiHelperResponse.Success("Employee and User Account created successfully", "Success");
        }
    }
}