using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.Mappings;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetEmployeeProfileByIdQuery(int employeeId) : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public int EmployeeId { get; } = employeeId;
    
    public class Handler(IEmployeeRepository employeeRepository, IApplicationDbContext appDbContext) : IRequestHandler<GetEmployeeProfileByIdQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetEmployeeProfileByIdQuery request, CancellationToken cancellationToken)
        {
            var profile = await employeeRepository.GetByIdAsync(request.EmployeeId);

            if (profile == null) throw new ApiException("Not found", (int)System.Net.HttpStatusCode.NotFound, "Employee not found");
            
            var lookups = await appDbContext.SystemLookups
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);

            var response = profile.ToProfileResponse(lookups);
            
            return ApiHelperResponse.Success("Employee Profile Retrieved Successfully", response);
        }
    }
}
