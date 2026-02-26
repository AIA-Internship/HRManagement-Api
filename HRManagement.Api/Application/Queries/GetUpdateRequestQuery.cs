using AutoMapper;
using MediatR;

using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Responses.Shared;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetUpdateRequestQuery(int? status) : IRequest<ApiResponse<List<EmployeeRequestResponseDto>>>
{
    public int? Status { get; set; } = status;
    
    public class Handler(IRequestRepository requestRepository, IMapper mapper, IApplicationDbContext appDbContext) : IRequestHandler<GetUpdateRequestQuery, ApiResponse<List<EmployeeRequestResponseDto>>>
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
            
            var response = mapper.Map<List<EmployeeRequestResponseDto>>(domainRequests);

            foreach (var item in response)
            {
                var domainRequest = domainRequests.FirstOrDefault(x => x.Id == item.RequestId);
                if (domainRequest != null)
                {
                    item.NewGender = lookups.FirstOrDefault(x => x.Category == "GENDER" && x.Value == domainRequest.NewGender)?.DisplayName ?? "Unknown";
                    item.NewMaritalStatus = lookups.FirstOrDefault(x => x.Category == "MARITAL_STATUS" && x.Value == domainRequest.NewMaritalStatus)?.DisplayName ?? "Unknown";
                    item.Status = lookups.FirstOrDefault(x => x.Category == "REQUEST_STATUS" && x.Value == domainRequest.Status)?.DisplayName ?? "Unknown";
                }
            }
            
            return ApiHelperResponse.Success("Employee Request Retrieved Successfully", response);
        }
    }
}
