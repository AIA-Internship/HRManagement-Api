using System.Net;
using AutoMapper;
using MediatR;

using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.Mappings;
using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;

namespace HRManagement.Api.Application.Queries;

public class GetEmployeeProfileByDisplayIdQuery(string displayId) : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public string DisplayId { get; } = displayId;

    public class Handler(IEmployeeRepository employeeRepository, IMapper mapper) : IRequestHandler<GetEmployeeProfileByDisplayIdQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetEmployeeProfileByDisplayIdQuery query, CancellationToken cancellationToken)
        {
            var employee = await employeeRepository.GetByDisplayIdAsync(query.DisplayId);
            if (employee == null)
            {
                throw new ApiException(
                    "Not Found", 
                    (int)HttpStatusCode.NotFound, 
                    ExceptionConstants.EmployeeNotFound
                );
            }

            var result = mapper.Map<EmployeeProfileResponseDto>(employee);
            return ApiHelperResponse.Success("Employee Profile Retrieved Successfully", result);
        }
    }
}
