using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Commands;

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
            
            if (request == null) throw new ApiException("Not Found", (int)System.Net.HttpStatusCode.NotFound, "Request Not Found");
            if (request.Status != 0) throw new ApiException("Bad Request", (int)System.Net.HttpStatusCode.BadRequest, "This request has already been processed");
            if (request.Employee.EmployeeEmail == hrEmail) throw new ApiException("Conflict", (int) System.Net.HttpStatusCode.Conflict, "You cannot approve your own request.");
            
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
