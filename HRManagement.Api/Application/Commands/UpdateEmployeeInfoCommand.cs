using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Responses.Shared;
using MediatR;

namespace HRManagement.Api.Application.Commands;

public class UpdateEmployeeInfoCommand(int employeeId, UpdateEmploymentInfoRequestDto commandDto) : IRequest<ApiResponse<string>>
{
    public int EmployeeId { get; } = employeeId;
    public UpdateEmploymentInfoRequestDto RequestDto { get; } = commandDto;
    
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService) : IRequestHandler<UpdateEmployeeInfoCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(UpdateEmployeeInfoCommand command, CancellationToken cancellationToken)
        {
            var employee = await employeeRepository.GetByIdAsync(command.EmployeeId);
            if (employee == null) throw new ApiException("Not found", 404, "Employee not found");
            
            var actionerId = currentUserService.UserId;
            var dto = command.RequestDto;
            
            employee.UpdateEmploymentInfo(
                dto.EmploymentStatus,
                dto.StartDate,
                dto.EmploymentType,
                dto.Department,
                dto.Position,
                dto.SupervisorName,
                actionerId
            );
            
            await employeeRepository.UpdateEmployeeAsync(employee);
            
            const string message = "Employee Employment Information Updated Successfully";
            return ApiHelperResponse.Success(message, message);
        }
    }
}