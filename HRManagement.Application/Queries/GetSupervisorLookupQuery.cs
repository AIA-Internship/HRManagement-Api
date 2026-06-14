using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Domain.Interfaces;

using MediatR;

namespace HRManagement.Application.Queries;

public record GetSupervisorLookupQuery() : IRequest<ApiResponse<List<SupervisorLookupResponseDto>>>;

public class GetSupervisorLookupQueryHandler(IEmployeeRepository employeeRepository) : IRequestHandler<GetSupervisorLookupQuery, ApiResponse<List<SupervisorLookupResponseDto>>>
{
    public async Task<ApiResponse<List<SupervisorLookupResponseDto>>> Handle(GetSupervisorLookupQuery request, CancellationToken cancellationToken)
    {
        var supervisors = await employeeRepository.GetSupervisorLookupAsync(cancellationToken);
        return ApiHelperResponse.Success("Retrieved supervisors lookup successfully", supervisors);
    }
}
