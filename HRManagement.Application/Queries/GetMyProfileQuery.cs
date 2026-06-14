using HRManagement.Api.Application.Mappings;
using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Interfaces;

namespace HRManagement.Application.Queries;

public class GetMyProfileQuery : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService, IApplicationDbContext appDbContext) : IRequestHandler<GetMyProfileQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
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
        
            var profile = await employeeRepository.GetByEmailAsync(email);
            if (profile == null)
            {
                throw new ApiException(
                    "Not found", 
                    StatusCodes.Status404NotFound, 
                    ExceptionConstants.EmployeeNotFound
                );
            }

            var lookups = await appDbContext.SystemLookups
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);

            var response = profile.ToProfileResponse(lookups);
            
            return ApiHelperResponse.Success("data retrieved successfully", response);
        }
    }
}
