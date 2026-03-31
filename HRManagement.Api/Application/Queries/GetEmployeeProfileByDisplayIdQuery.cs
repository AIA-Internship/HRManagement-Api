using System.Net;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.Mappings;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetEmployeeProfileByDisplayIdQuery(string employeeDisplayId) : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public string EmployeeDisplayId { get; } = employeeDisplayId;
    
    public class Handler(IEmployeeRepository employeeRepository, IApplicationDbContext appDbContext) : IRequestHandler<GetEmployeeProfileByDisplayIdQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetEmployeeProfileByDisplayIdQuery request, CancellationToken cancellationToken)
        {
            var profile = await employeeRepository.GetByDisplayIdAsync(request.EmployeeDisplayId);

            if (profile == null)
            {
                throw new ApiException(
                    "Not Found", 
                    (int)HttpStatusCode.NotFound, 
                    ExceptionConstants.EmployeeNotFound
                );
            }
            
            var lookups = await appDbContext.SystemLookups
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);

            var response = profile.ToProfileResponse(lookups);
            
            return ApiHelperResponse.Success("Employee Profile Retrieved Successfully", response);
        }
    }
}
