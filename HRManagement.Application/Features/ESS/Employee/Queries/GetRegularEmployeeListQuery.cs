using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetSupervisorEmployeeListQuery() : IRequest<Result<List<EmployeeListResponseDto>>>;

internal sealed class GetSupervisorEmployeeListQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<GetSupervisorEmployeeListQueryHandler> logger) : IRequestHandler<GetSupervisorEmployeeListQuery, Result<List<EmployeeListResponseDto>>>
{
    public async Task<Result<List<EmployeeListResponseDto>>> Handle(GetSupervisorEmployeeListQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetSupervisorEmployeeListQueryHandler));
        var data = await employeeRepository.GetEmployeesByRoleAsync("supervisor", cancellationToken);
        return Result.Success(data);
    }
}