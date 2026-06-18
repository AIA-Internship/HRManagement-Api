using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetEmployeeListQuery() : IRequest<Result<List<EmployeeListResponseDto>>>;

internal sealed class GetEmployeeListQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<GetEmployeeListQueryHandler> logger) : IRequestHandler<GetEmployeeListQuery, Result<List<EmployeeListResponseDto>>>
{
    public async Task<Result<List<EmployeeListResponseDto>>> Handle(GetEmployeeListQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetEmployeeListQueryHandler));

        var data = await employeeRepository.GetAllEmployeesAsync(cancellationToken);

        return Result.Success(data);
    }
}