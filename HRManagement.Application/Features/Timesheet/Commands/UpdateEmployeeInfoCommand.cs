using HRManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Commands.Dto;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Commands;

public class UpdateEmployeeInfoCommand(string EmployeeDisplayId, UpdateEmploymentInfoRequestDto commandDto) : IRequest<ApiResponse<string>>
{
    public string EmployeeDisplayId { get; } = EmployeeDisplayId;
    public UpdateEmploymentInfoRequestDto RequestDto { get; } = commandDto;
    
    public class Handler(IApplicationDbContext context, ICurrentUserService currentUserService) : IRequestHandler<UpdateEmployeeInfoCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(UpdateEmployeeInfoCommand command, CancellationToken cancellationToken)
        {
            var employee = await context.Employee
                .Include(e => e.EmploymentInformation)
                .FirstOrDefaultAsync(e => e.EmploymentInformation != null && e.EmploymentInformation.DisplayId == command.EmployeeDisplayId, cancellationToken);
                
            if (employee == null || employee.EmploymentInformation == null)
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
                var supervisor = await context.EmploymentInformation
                    .FirstOrDefaultAsync(ei => ei.DisplayId == dto.SupervisorDisplayId, cancellationToken);
                if (supervisor != null)
                {
                    supervisorId = supervisor.EmployeeId;
                }
            }
            
            employee.EmploymentInformation.UpdateDetails(
                dto.EmploymentStatus,
                dto.StartDate,
                dto.EmploymentType,
                dto.Department,
                dto.Position,
                supervisorId,
                dto.EmployeeDisplayId,
                actionerId
            );
            
            await context.SaveChangesAsync(cancellationToken);
            
            const string message = "Employee Employment Information Updated Successfully";
            return ApiHelperResponse.Success<string>(message, message);
        }
    }
}




