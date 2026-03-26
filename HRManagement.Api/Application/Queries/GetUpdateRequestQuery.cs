using MediatR;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.Mappings;
using HRManagement.Api.Domain.Models.Response.Shared;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetUpdateRequestQuery(int? status) : IRequest<ApiResponse<List<EmployeeRequestResponseDto>>>
{
    public int? Status { get; set; } = status;
    
    public class Handler(IRequestRepository requestRepository, IApplicationDbContext appDbContext) : IRequestHandler<GetUpdateRequestQuery, ApiResponse<List<EmployeeRequestResponseDto>>>
    {
        public async Task<ApiResponse<List<EmployeeRequestResponseDto>>> Handle(GetUpdateRequestQuery request,
            CancellationToken cancellationToken)
        {
            var domainRequests = await requestRepository.GetEmployeeUpdateRequestAsync(request.Status);
            if (domainRequests.Count == 0) throw new ApiException("Nothing found", (int)System.Net.HttpStatusCode.NotFound, "No update request found");
            
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
