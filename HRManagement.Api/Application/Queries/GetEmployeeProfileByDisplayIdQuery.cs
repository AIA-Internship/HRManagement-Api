using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using AutoMapper;
using MediatR;

namespace HRManagement.Api.Application.Queries;

public class GetEmployeeProfileByDisplayIdQuery(string displayId) : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public string DisplayId { get; } = displayId;

    public class Handler(IEmployeeRepository employeeRepository, IMapper mapper) : IRequestHandler<GetEmployeeProfileByDisplayIdQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetEmployeeProfileByDisplayIdQuery query, CancellationToken cancellationToken)
        {
            var employee = await employeeRepository.GetByDisplayIdAsync(query.DisplayId);
            if (employee == null) throw new ApiException("Not found", 404, "Employee not found");

            var response = mapper.Map<EmployeeProfileResponseDto>(employee);
            return ApiHelperResponse.Success(response);
        }
    }
}
