using HRManagement.Api.Application.Interfaces;
using AutoMapper;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetEmployeeListQuery : IRequest<ApiResponse<List<EmployeeListItemDto>>>
{
    public class Handler(IEmployeeRepository employeeRepository, IMapper mapper, IApplicationDbContext appDbContext) : IRequestHandler<GetEmployeeListQuery, ApiResponse<List<EmployeeListItemDto>>>
    {
        public async Task<ApiResponse<List<EmployeeListItemDto>>> Handle(GetEmployeeListQuery request, CancellationToken cancellationToken)
        {
            var employees =  await employeeRepository.GetAllEmployeesAsync();
            var response = mapper.Map<List<EmployeeListItemDto>>(employees);
            
            var lookups = await appDbContext.SystemLookups 
                .AsNoTracking() 
                .Where(x => x.IsActive && x.Category == "EMPLOYMENT_STATUS") 
                .ToListAsync(cancellationToken);

            for (int i = 0; i < employees.Count; i++)
            {
                var employmentStatus = employees[i].EmploymentInformation?.EmploymentStatus;
                response[i].EmployeeStatus = lookups.FirstOrDefault(x => x.Value == employmentStatus)?.DisplayName ?? "Unknown";
            }
            
            return ApiHelperResponse.Success("Employee List Showed Successfully", response);
        }
    }
}
