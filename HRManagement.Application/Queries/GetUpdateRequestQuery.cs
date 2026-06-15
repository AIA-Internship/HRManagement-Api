using MediatR;
using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Mappings;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Interfaces;

namespace HRManagement.Application.Queries;

public class GetUpdateRequestQuery(int? status) : IRequest<ApiResponse<List<EmployeeRequestResponseDto>>>
{
    public int? Status { get; set; } = status;
    
    public class Handler(IRequestRepository requestRepository, IApplicationDbContext appDbContext) : IRequestHandler<GetUpdateRequestQuery, ApiResponse<List<EmployeeRequestResponseDto>>>
    {
        public async Task<ApiResponse<List<EmployeeRequestResponseDto>>> Handle(GetUpdateRequestQuery request,
            CancellationToken cancellationToken)
        {
            var domainRequests = await requestRepository.GetEmployeeUpdateRequestAsync(request.Status);
            if (domainRequests.Count == 0)
            {
                throw new ApiException(
                    "Nothing found", 
                    StatusCodes.Status404NotFound, 
                    ExceptionConstants.UpdateRequestNotFound
                );
            }
            
            var lookups = await appDbContext.SystemLookups
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);

            var response = domainRequests
                .Select(domainRequest => domainRequest.ToEmployeeRequestResponse(lookups))
                .ToList();
            
            return ApiHelperResponse.Success("Employee Request Retrieved Successfully", response);
        }
    }
}
