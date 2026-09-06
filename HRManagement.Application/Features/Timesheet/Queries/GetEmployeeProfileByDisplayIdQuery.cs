using HRManagement.Domain.Interfaces;
using System.Net;
using AutoMapper;
using MediatR;

using HRManagement.Domain.Models.Response;
using HRManagement.Application.Interfaces;
using HRManagement.Application.Mappings;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response.Shared;

namespace HRManagement.Application.Queries;

public class GetEmployeeProfileByDisplayIdQuery(string displayId) : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public string DisplayId { get; } = displayId;

    public class Handler(IEmployeeRepository employeeRepository) : IRequestHandler<GetEmployeeProfileByDisplayIdQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetEmployeeProfileByDisplayIdQuery query, CancellationToken cancellationToken)
        {
            var employee = await employeeRepository.GetProfileByDisplayIdAsync(query.DisplayId);
            if (employee == null)
            {
                throw new ApiException(
                    "Not Found", 
                    (int)HttpStatusCode.NotFound, 
                    ExceptionConstants.EmployeeNotFound
                );
            }

            return ApiHelperResponse.Success("Employee Profile Retrieved Successfully", employee);
        }
    }
}
