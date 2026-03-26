using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.Mappings;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetMyProfileQuery : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService, IApplicationDbContext appDbContext) : IRequestHandler<GetMyProfileQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var email = currentUserService.Email;
            if (string.IsNullOrEmpty(email)) throw new ApiException("Unauthorized", (int)System.Net.HttpStatusCode.Unauthorized, "User not authenticated");
        
            var profile = await employeeRepository.GetByEmailAsync(email);
            if (profile == null) throw new ApiException("Not found", (int)System.Net.HttpStatusCode.NotFound, "User not found");

            var lookups = await appDbContext.SystemLookups
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);

            var response = profile.ToProfileResponse(lookups);
            
            return ApiHelperResponse.Success("data retrieved successfully", response);
        }
    }
}
