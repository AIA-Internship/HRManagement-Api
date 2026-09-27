using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetRegularEmployeeListQuery() : IRequest<Result<List<EmployeeListResponseDto>>>;

internal sealed class GetRegularEmployeeListQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<GetRegularEmployeeListQueryHandler> logger) : IRequestHandler<GetRegularEmployeeListQuery, Result<List<EmployeeListResponseDto>>>
{
    public async Task<Result<List<EmployeeListResponseDto>>> Handle(GetRegularEmployeeListQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetRegularEmployeeListQueryHandler));

        var data = await employeeRepository.GetEmployeesByRoleAsync("employee", cancellationToken);

        return Result.Success(data);
    }
}