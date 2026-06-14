using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Interfaces;

using MediatR;

namespace HRManagement.Application.Commands;

public class ReviewUpdateCommand(ReviewUpdateRequestDto decision) : IRequest<ApiResponse<string>>
{
    private ReviewUpdateRequestDto Decision { get; } = decision;

    public class Handler(IEmployeeRepository employeeRepository, IRequestRepository requestRepository, ICurrentUserService currentUserService)
        : IRequestHandler<ReviewUpdateCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(ReviewUpdateCommand command, CancellationToken ct)
        {
            var hrActionerId = currentUserService.UserId;
            var hrEmail = currentUserService.Email;
            var request = await requestRepository.GetEmployeeUpdateRequestByIdAsync(command.Decision.RequestId);

            if (request == null)
            {
                throw new ApiException(
                    "Not Found", 
                    StatusCodes.Status404NotFound, 
                    ExceptionConstants.NotFound
                );
            }

            if (request.Status != 0)
            {
                throw new ApiException(
                    "Bad Request", 
                    StatusCodes.Status400BadRequest, 
                    ExceptionConstants.BadRequest
                );
            }

            if (request.Employee.EmployeeEmail == hrEmail)
            {
                throw new ApiException(
                    "Conflict", (int) 
                    StatusCodes.Status409Conflict, 
                    ExceptionConstants.Conflict
                );
            }
            
            if (command.Decision.IsApproved)
            {
                request.Approve(command.Decision.Reason, hrActionerId);
                request.Employee.ApplyUpdate(request, hrActionerId);
        
                await employeeRepository.UpdateEmployeeAsync(request.Employee);
            }
            else
            {
                request.Reject(command.Decision.Reason, hrActionerId);
            }
            
            await requestRepository.UpdateRequestStatusAsync(request);
            var result = command.Decision.IsApproved ? "Approved" : "Rejected";
            return ApiHelperResponse.Success("Review Processed Successfully", result);
        }
    }
}
