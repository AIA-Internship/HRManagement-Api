using HRManagement.Api.Application.Interfaces;
<<<<<<< HEAD
using AutoMapper;
=======
using HRManagement.Api.Application.Mappings;
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetMyProfileQuery : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
<<<<<<< HEAD
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService, IMapper mapper, IApplicationDbContext appDbContext) : IRequestHandler<GetMyProfileQuery, ApiResponse<EmployeeProfileResponseDto>>
=======
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService, IApplicationDbContext appDbContext) : IRequestHandler<GetMyProfileQuery, ApiResponse<EmployeeProfileResponseDto>>
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var email = currentUserService.Email;
            if (string.IsNullOrEmpty(email)) throw new ApiException("Unauthorized", (int)System.Net.HttpStatusCode.Unauthorized, "User not authenticated");
        
            var profile = await employeeRepository.GetByEmailAsync(email);
            if (profile == null) throw new ApiException("Not found", (int)System.Net.HttpStatusCode.NotFound, "User not found");

<<<<<<< HEAD
            var response = mapper.Map<EmployeeProfileResponseDto>(profile);
            
=======
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
            var lookups = await appDbContext.SystemLookups
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);
<<<<<<< HEAD
            
            response.Gender = lookups.FirstOrDefault(x => x.Category == "GENDER" && x.Value == profile.Gender)?.DisplayName ?? "Unknown";
            response.MaritalStatus = lookups.FirstOrDefault(x => x.Category == "MARITAL_STATUS" && x.Value == profile.MaritalStatus)?.DisplayName ?? "Unknown";

            if (profile.EmploymentInformation != null)
            {
                response.EmployeeStatus = lookups.FirstOrDefault(x => x.Category == "EMPLOYMENT_STATUS" && x.Value == profile.EmploymentInformation.EmploymentStatus)?.DisplayName ?? "Unknown";
                response.EmploymentType = lookups.FirstOrDefault(x => x.Category == "EMPLOYMENT_TYPE" && x.Value == profile.EmploymentInformation.EmploymentType)?.DisplayName ?? "Unknown";
            }
=======

            var response = profile.ToProfileResponse(lookups);
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
            
            return ApiHelperResponse.Success("data retrieved successfully", response);
        }
    }
}
