using HRManagement.Api.Application.Interfaces;
using AutoMapper;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Responses.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetEmployeeProfileByIdQuery(int employeeId) : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public int EmployeeId { get; } = employeeId;
    
    public class Handler(IEmployeeRepository employeeRepository, IMapper mapper, IApplicationDbContext appDbContext) : IRequestHandler<GetEmployeeProfileByIdQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetEmployeeProfileByIdQuery request, CancellationToken cancellationToken)
        {
            var profile = await employeeRepository.GetByIdAsync(request.EmployeeId);

            if (profile == null) throw new ApiException("Not found", (int)System.Net.HttpStatusCode.NotFound, "Employee not found");

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
            
            return ApiHelperResponse.Success("Employee Profile Retrieved Successfully", response);
        }
    }
}
