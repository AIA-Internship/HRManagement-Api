<<<<<<< HEAD
using AutoMapper;
using MediatR;

using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
=======
using MediatR;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.Mappings;
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
using HRManagement.Api.Domain.Models.Response.Shared;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetUpdateRequestQuery(int? status) : IRequest<ApiResponse<List<EmployeeRequestResponseDto>>>
{
    public int? Status { get; set; } = status;
    
<<<<<<< HEAD
    public class Handler(IRequestRepository requestRepository, IMapper mapper, IApplicationDbContext appDbContext) : IRequestHandler<GetUpdateRequestQuery, ApiResponse<List<EmployeeRequestResponseDto>>>
=======
    public class Handler(IRequestRepository requestRepository, IApplicationDbContext appDbContext) : IRequestHandler<GetUpdateRequestQuery, ApiResponse<List<EmployeeRequestResponseDto>>>
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
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
<<<<<<< HEAD
            
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
=======

            var response = domainRequests
                .Select(domainRequest => domainRequest.ToEmployeeRequestResponse(lookups))
                .ToList();
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
            
            return ApiHelperResponse.Success("Employee Request Retrieved Successfully", response);
        }
    }
}
