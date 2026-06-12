using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Application.Interfaces;

using MediatR;

namespace HRManagement.Application.Queries;

public record GetSupervisorLookupQuery() : IRequest<ApiResponse<List<SupervisorLookupDto>>>;

public class GetSupervisorLookupQueryHandler(IEmployeeRepository employeeRepository) : IRequestHandler<GetSupervisorLookupQuery, ApiResponse<List<SupervisorLookupDto>>>
{
    public async Task<ApiResponse<List<SupervisorLookupDto>>> Handle(GetSupervisorLookupQuery request, CancellationToken cancellationToken)
    {
        var supervisors = await employeeRepository.GetSupervisorLookupAsync(cancellationToken);
        return ApiHelperResponse.Success("Retrieved supervisors lookup successfully", supervisors);
    }
}
