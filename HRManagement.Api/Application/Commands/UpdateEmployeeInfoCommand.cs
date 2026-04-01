using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Commands;

public class UpdateEmployeeInfoCommand(string employeeDisplayId, UpdateEmploymentInfoRequestDto commandDto) : IRequest<ApiResponse<string>>
{
    public string EmployeeDisplayId { get; } = employeeDisplayId;
    public UpdateEmploymentInfoRequestDto RequestDto { get; } = commandDto;
    
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService) : IRequestHandler<UpdateEmployeeInfoCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(UpdateEmployeeInfoCommand command, CancellationToken cancellationToken)
        {
            var employee = await employeeRepository.GetByDisplayIdAsync(command.EmployeeDisplayId);
            if (employee == null)
            {
                throw new ApiException(
                    "Not found", 
                    StatusCodes.Status404NotFound, 
                    ExceptionConstants.EmployeeNotFound
                );
            }
            
            var actionerId = currentUserService.UserId;
            var dto = command.RequestDto;

            int? supervisorId = null;
            if (!string.IsNullOrWhiteSpace(dto.SupervisorDisplayId))
            {
                var supervisor = await employeeRepository.GetByDisplayIdAsync(dto.SupervisorDisplayId);
                supervisorId = supervisor?.Id;
            }
            
            employee.UpdateEmploymentInfo(
                dto.EmploymentStatus,
                dto.StartDate,
                dto.EmploymentType,
                dto.Department,
                dto.Position,
                supervisorId,
                dto.EmployeeDisplayId,
                actionerId
            );
            
            await employeeRepository.UpdateEmployeeAsync(employee);
            
            const string message = "Employee Employment Information Updated Successfully";
            return ApiHelperResponse.Success(message, message);
        }
    }
}