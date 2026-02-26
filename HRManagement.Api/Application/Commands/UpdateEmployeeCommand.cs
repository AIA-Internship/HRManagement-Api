using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using AutoMapper;
using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Commands;

public class UpdateEmployeeCommand(UpdateEmployeeRequestDto commandDto) : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public UpdateEmployeeRequestDto RequestDto { get; } = commandDto;

    public class Handler(IEmployeeRepository employeeRepository, IRequestRepository requestRepository, ICurrentUserService currentUserService, IMapper mapper) : IRequestHandler<UpdateEmployeeCommand, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken)
        {
            var email = currentUserService.Email;
            if (string.IsNullOrEmpty(email)) throw new ApiException("Unauthorized", 401, "User not authenticated");
            
            var employee = await employeeRepository.GetByEmailAsync(email);
            if (employee == null) throw new ApiException("Not found", 404, "Employee not found");
            
            var actionerId = currentUserService.UserId;
            var request = new EmployeeUpdateRequest(employee.Id, command.RequestDto, actionerId);
            
            await requestRepository.SubmitUpdateRequestAsync(request);
            
            var response = mapper.Map<EmployeeProfileResponseDto>(employee);
            
            // 4. (Optional) If you want the response to show the newly requested data 
            // instead of the old approved data, you can overwrite it manually here:
            response.FullName = string.IsNullOrWhiteSpace(request.NewFullName) ? response.FullName : request.NewFullName;
            
            return ApiHelperResponse.Success("Update request submitted successfully. Pending HR Approval.", response);
        }
    }
}