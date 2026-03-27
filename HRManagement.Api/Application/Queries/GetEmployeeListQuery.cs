using HRManagement.Api.Application.Interfaces;
<<<<<<< HEAD
using AutoMapper;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
=======
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Mappings;
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public class GetEmployeeListQuery : IRequest<ApiResponse<List<EmployeeListItemDto>>>
{
<<<<<<< HEAD
    public class Handler(IEmployeeRepository employeeRepository, IMapper mapper, IApplicationDbContext appDbContext) : IRequestHandler<GetEmployeeListQuery, ApiResponse<List<EmployeeListItemDto>>>
=======
    public class Handler(IEmployeeRepository employeeRepository, IApplicationDbContext appDbContext) : IRequestHandler<GetEmployeeListQuery, ApiResponse<List<EmployeeListItemDto>>>
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
    {
        public async Task<ApiResponse<List<EmployeeListItemDto>>> Handle(GetEmployeeListQuery request, CancellationToken cancellationToken)
        {
            var employees =  await employeeRepository.GetAllEmployeesAsync();
<<<<<<< HEAD
            var response = mapper.Map<List<EmployeeListItemDto>>(employees);
=======
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
            
            var lookups = await appDbContext.SystemLookups 
                .AsNoTracking() 
                .Where(x => x.IsActive && x.Category == "EMPLOYMENT_STATUS") 
                .ToListAsync(cancellationToken);

<<<<<<< HEAD
            for (int i = 0; i < employees.Count; i++)
            {
                var employmentStatus = employees[i].EmploymentInformation?.EmploymentStatus;
                response[i].EmployeeStatus = lookups.FirstOrDefault(x => x.Value == employmentStatus)?.DisplayName ?? "Unknown";
            }
=======
            var response = employees 
                .Select(employee => employee.ToEmployeeListResponse(lookups)) 
                .ToList();
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
            
            return ApiHelperResponse.Success("Employee List Showed Successfully", response);
        }
    }
}
