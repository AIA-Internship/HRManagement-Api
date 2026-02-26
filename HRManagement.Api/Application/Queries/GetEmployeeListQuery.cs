using HRManagement.Api.Application.Interfaces;
using AutoMapper;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Queries;

public class GetEmployeeListQuery : IRequest<ApiResponse<List<EmployeeListItemDto>>>
{
    public class Handler(IEmployeeRepository employeeRepository, IMapper mapper) : IRequestHandler<GetEmployeeListQuery, ApiResponse<List<EmployeeListItemDto>>>
    {
        public async Task<ApiResponse<List<EmployeeListItemDto>>> Handle(GetEmployeeListQuery request, CancellationToken cancellationToken)
        {
            var employees =  await employeeRepository.GetAllEmployeesAsync();
            var response = mapper.Map<List<EmployeeListItemDto>>(employees);

            return ApiHelperResponse.Success("Employee List Showed Successfully", response);
        }
    }
}
