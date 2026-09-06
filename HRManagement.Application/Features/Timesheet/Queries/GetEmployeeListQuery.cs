using HRManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Queries.Dto;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Queries;

public class GetEmployeeListQuery : IRequest<ApiResponse<List<EmployeeListItemDto>>>
{
    public class Handler(IEmployeeRepository employeeRepository) : IRequestHandler<GetEmployeeListQuery, ApiResponse<List<EmployeeListItemDto>>>
    {
        public async Task<ApiResponse<List<EmployeeListItemDto>>> Handle(GetEmployeeListQuery request, CancellationToken cancellationToken)
        {
            var employees = await employeeRepository.GetAllEmployeesAsync(cancellationToken);
            if (employees == null || employees.Count == 0)
            {
                throw new ApiException("Employee List not Found", StatusCodes.Status404NotFound, ExceptionConstants.EmployeeNotFound);
            }
            
            var response = employees.Select(employee => new EmployeeListItemDto
            {
                FullName = employee.FullName,
                EmployeeDisplayId = employee.EmployeeDisplayId,
                Department = employee.Department,
                Position = employee.Position,
                EmployeeStatus = employee.TypeName
            }).ToList();

            return ApiHelperResponse.Success("Employee List Data Retrieved successfully", response);
        }
    }
}
