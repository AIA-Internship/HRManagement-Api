using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Queries;

public class GetSupervisorLookupQuery : IRequest<ApiResponse<List<SupervisorLookupDto>>>
{
    public class Handler(IEmployeeRepository employeeRepository) : IRequestHandler<GetSupervisorLookupQuery, ApiResponse<List<SupervisorLookupDto>>>
    {
        public async Task<ApiResponse<List<SupervisorLookupDto>>> Handle(GetSupervisorLookupQuery request, CancellationToken cancellationToken)
        {
            var supervisors = await employeeRepository.GetSupervisorLookupAsync(cancellationToken);
            return ApiHelperResponse.Success("Retrieved supervisors lookup successfully", supervisors);
        }
    }
}
