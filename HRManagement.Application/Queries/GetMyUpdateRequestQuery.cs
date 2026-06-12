using HRManagement.Api.Application.Mappings;
using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Application.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.Application.Queries;

public class GetMyUpdateRequestQuery(int? status) : IRequest<ApiResponse<List<EmployeeRequestResponseDto>>>
{
    public int? Status { get; set; } = status;
    
    public class Handler(
        IRequestRepository requestRepository, 
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService,
        IEmployeeRepository employeeRepository) : IRequestHandler<GetMyUpdateRequestQuery, ApiResponse<List<EmployeeRequestResponseDto>>>
    {
        public async Task<ApiResponse<List<EmployeeRequestResponseDto>>> Handle(GetMyUpdateRequestQuery request,
            CancellationToken cancellationToken)
        {
            var email = currentUserService.Email;
            if (string.IsNullOrEmpty(email))
            {
                throw new ApiException(
                    "Unauthorized", 
                    StatusCodes.Status401Unauthorized, 
                    ExceptionConstants.NotAuthorizedExcepction
                );
            }

            var employee = await employeeRepository.GetByEmailAsync(email);
            if (employee == null)
            {
                throw new ApiException(
                    "Employee not found", 
                    StatusCodes.Status404NotFound, 
                    ExceptionConstants.EmployeeNotFound
                );
            }

            var domainRequests = await requestRepository.GetEmployeeUpdateRequestAsync(request.Status, employee.Id);
            
            var lookups = await appDbContext.SystemLookups
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);

            var response = domainRequests
                .Select(domainRequest => domainRequest.ToEmployeeRequestResponse(lookups))
                .ToList();
            
            return ApiHelperResponse.Success("My Employee Requests Retrieved Successfully", response);
        }
    }
}
