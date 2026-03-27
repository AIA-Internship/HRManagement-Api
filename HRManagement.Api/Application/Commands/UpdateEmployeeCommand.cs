using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
<<<<<<< HEAD
using AutoMapper;
using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
=======
using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Mappings;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a

namespace HRManagement.Api.Application.Commands;

public class UpdateEmployeeCommand(UpdateEmployeeRequestDto commandDto) : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public UpdateEmployeeRequestDto RequestDto { get; } = commandDto;

<<<<<<< HEAD
    public class Handler(IEmployeeRepository employeeRepository, IRequestRepository requestRepository, ICurrentUserService currentUserService, IMapper mapper) : IRequestHandler<UpdateEmployeeCommand, ApiResponse<EmployeeProfileResponseDto>>
=======
    public class Handler(IEmployeeRepository employeeRepository, IRequestRepository requestRepository, ICurrentUserService currentUserService, IApplicationDbContext appDbContext) : IRequestHandler<UpdateEmployeeCommand, ApiResponse<EmployeeProfileResponseDto>>
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
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
<<<<<<< HEAD
            
            var response = mapper.Map<EmployeeProfileResponseDto>(employee);
            return ApiHelperResponse.Success("Update request submitted successfully. Pending HR Approval.", response);
        }
    }
}
=======

            var lookups = await appDbContext.SystemLookups
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);
            
            var response = employee.ToProfileResponse(lookups);
            return ApiHelperResponse.Success("Update request submitted successfully. Pending HR Approval.", response);
        }
    }
}
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
