using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Application.Queries;

public class GetSupervisorLookupQuery : IRequest<ApiResponse<List<SupervisorLookupResponseDto>>>
{
    public class Handler(IEmployeeRepository employeeRepository) : IRequestHandler<GetSupervisorLookupQuery, ApiResponse<List<SupervisorLookupResponseDto>>>
    {
        public async Task<ApiResponse<List<SupervisorLookupResponseDto>>> Handle(GetSupervisorLookupQuery request, CancellationToken cancellationToken)
        {
            var supervisors = await employeeRepository.GetSupervisorLookupAsync(cancellationToken);
            return ApiHelperResponse.Success("Retrieved supervisors lookup successfully", supervisors);
        }
    }
}








