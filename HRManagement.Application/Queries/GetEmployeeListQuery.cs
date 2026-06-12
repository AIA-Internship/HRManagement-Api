using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Application.Mappings;
using HRManagement.Application.Interfaces;

namespace HRManagement.Application.Queries;

public class GetEmployeeListQuery : IRequest<ApiResponse<List<EmployeeListItemDto>>>
{
    public class Handler(IEmployeeRepository employeeRepository, IApplicationDbContext appDbContext) : IRequestHandler<GetEmployeeListQuery, ApiResponse<List<EmployeeListItemDto>>>
    {
        public async Task<ApiResponse<List<EmployeeListItemDto>>> Handle(GetEmployeeListQuery request, CancellationToken cancellationToken)
        {
            var employees =  await employeeRepository.GetAllEmployeesAsync();
            
            var lookups = await appDbContext.SystemLookups 
                .AsNoTracking() 
                .Where(x => x.IsActive && x.Category == "EMPLOYMENT_STATUS") 
                .ToListAsync(cancellationToken);

            var response = employees 
                .Select(employee => employee.ToEmployeeListResponse(lookups)) 
                .ToList();
            
            return ApiHelperResponse.Success("Employee List Showed Successfully", response);
        }
    }
}
