using HRManagement.Api.Application.Interfaces;
using AutoMapper;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetMyProfileQuery : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService, IMapper mapper, IApplicationDbContext appDbContext) : IRequestHandler<GetMyProfileQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var email = currentUserService.Email;
            if (string.IsNullOrEmpty(email)) throw new ApiException("Unauthorized", (int)System.Net.HttpStatusCode.Unauthorized, "User not authenticated");
        
            var profile = await employeeRepository.GetByEmailAsync(email);
            if (profile == null) throw new ApiException("Not found", (int)System.Net.HttpStatusCode.NotFound, "User not found");

            var response = mapper.Map<EmployeeProfileResponseDto>(profile);
            
            var lookups = await appDbContext.SystemLookups
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);
            
            response.Gender = lookups.FirstOrDefault(x => x.Category == "GENDER" && x.Value == profile.Gender)?.DisplayName ?? "Unknown";
            response.MaritalStatus = lookups.FirstOrDefault(x => x.Category == "MARITAL_STATUS" && x.Value == profile.MaritalStatus)?.DisplayName ?? "Unknown";

            if (profile.EmploymentInformation != null)
            {
                response.EmployeeStatus = lookups.FirstOrDefault(x => x.Category == "EMPLOYMENT_STATUS" && x.Value == profile.EmploymentInformation.EmploymentStatus)?.DisplayName ?? "Unknown";
                response.EmploymentType = lookups.FirstOrDefault(x => x.Category == "EMPLOYMENT_TYPE" && x.Value == profile.EmploymentInformation.EmploymentType)?.DisplayName ?? "Unknown";
            }
            
            return ApiHelperResponse.Success("data retrieved successfully", response);
        }
    }
}
