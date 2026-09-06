using HRManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HRManagement.Domain.Models.Response;

namespace HRManagement.Application.Queries;

public class GetUpdateRequestQuery(int? status) : IRequest<ApiResponse<List<EmployeeRequestResponseDto>>>
{
    public int? Status { get; set; } = status;
    
    public class Handler(IRequestRepository requestRepository) : IRequestHandler<GetUpdateRequestQuery, ApiResponse<List<EmployeeRequestResponseDto>>>
    {
        public async Task<ApiResponse<List<EmployeeRequestResponseDto>>> Handle(GetUpdateRequestQuery request,
            CancellationToken cancellationToken)
        {
            var domainRequests = await requestRepository.GetMyEmployeeUpdateRequestAsync(request.Status, null, cancellationToken);
            if (domainRequests == null || domainRequests.Count == 0)
            {
                throw new ApiException("Nothing found", StatusCodes.Status404NotFound, ExceptionConstants.UpdateRequestNotFound);
            }
            
            return ApiHelperResponse.Success("data retrieved successfully", domainRequests);
        }
    }
}
