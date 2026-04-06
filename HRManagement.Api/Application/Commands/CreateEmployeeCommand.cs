using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using System.Text.RegularExpressions;
using HRManagement.Api.Application.Mappings;
using MediatR;
using HRManagement.Api.Repositories.LeaveManagementRepositories;
using HRManagement.Api.Application.Interfaces.LeaveManagementInterface;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveBalance;

namespace HRManagement.Api.Application.Commands;

public class CreateEmployeeCommand(CreateEmployeeRequestDto commandDto) : IRequest<ApiResponse<string>>
{
    public CreateEmployeeRequestDto RequestDto { get; } = commandDto;
    
    public class Handler(ILeaveBalanceRepository leaveRepo, IEmployeeRepository employeeRepository, ICurrentUserService currentUserService, IPasswordHasher passwordHasher) : IRequestHandler<CreateEmployeeCommand, ApiResponse<string>>
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

                int? supervisorId = null;
                if (!string.IsNullOrWhiteSpace(dto.EmploymentInformation.SupervisorDisplayId))
                {
                    var supervisor = await employeeRepository.GetByDisplayIdAsync(dto.EmploymentInformation.SupervisorDisplayId);
                    supervisorId = supervisor?.Id;
                }
                employmentInfo = dto.EmploymentInformation.ToEntity(displayId, supervisorId, actionerId);
            }
            
            var emergencyContacts = dto.EmergencyContacts.ToEntityList(actionerId);
            var employee = dto.ToEntity(actionerId, employmentInfo, emergencyContacts);
            
            var hashedPassword = passwordHasher.Hash(dto.DefaultPassword);
            var user = new User(dto.EmployeeEmail, hashedPassword, dto.Role, actionerId);
            
            await employeeRepository.AddEmployeeAsync(user, employee);
            await leaveRepo.createLeaveBalance(new LeaveBalanceModel { EmployeeId= user.Id, LeaveBalance = 0 });

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