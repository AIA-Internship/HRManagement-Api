using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Commands;

<<<<<<< HEAD
public class UpdateEmployeeInfoCommand(int employeeId, UpdateEmploymentInfoRequestDto commandDto) : IRequest<ApiResponse<string>>
{
    public int EmployeeId { get; } = employeeId;
=======
public class UpdateEmployeeInfoCommand(string employeeDisplayId, UpdateEmploymentInfoRequestDto commandDto) : IRequest<ApiResponse<string>>
{
    public string EmployeeDisplayId { get; } = employeeDisplayId;
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
    public UpdateEmploymentInfoRequestDto RequestDto { get; } = commandDto;
    
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService) : IRequestHandler<UpdateEmployeeInfoCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(UpdateEmployeeInfoCommand command, CancellationToken cancellationToken)
        {
<<<<<<< HEAD
            var employee = await employeeRepository.GetByIdAsync(command.EmployeeId);
=======
            var employee = await employeeRepository.GetByDisplayIdAsync(command.EmployeeDisplayId);
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
            if (employee == null) throw new ApiException("Not found", 404, "Employee not found");
            
            var actionerId = currentUserService.UserId;
            var dto = command.RequestDto;
<<<<<<< HEAD
=======

            int? supervisorId = null;
            if (!string.IsNullOrWhiteSpace(dto.SupervisorDisplayId))
            {
                var supervisor = await employeeRepository.GetByDisplayIdAsync(dto.SupervisorDisplayId);
                supervisorId = supervisor?.Id;
            }
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
            
            employee.UpdateEmploymentInfo(
                dto.EmploymentStatus,
                dto.StartDate,
                dto.EmploymentType,
                dto.Department,
                dto.Position,
<<<<<<< HEAD
                dto.SupervisorName,
=======
                supervisorId,
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
                dto.EmployeeDisplayId,
                actionerId
            );
            
            await employeeRepository.UpdateEmployeeAsync(employee);
            
            const string message = "Employee Employment Information Updated Successfully";
            return ApiHelperResponse.Success(message, message);
        }
    }
}