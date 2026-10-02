using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetEmployeeRolesQuery
    : IRequest<Result<List<PositionLookupDto>>>;

internal sealed class GetEmployeeRolesQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<GetEmployeeRolesQueryHandler> logger)
    : IRequestHandler<GetEmployeeRolesQuery, Result<List<PositionLookupDto>>>
{
    public async Task<Result<List<PositionLookupDto>>> Handle(
        GetEmployeeRolesQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler : {HandlerName}",
            nameof(GetEmployeeRolesQueryHandler));

        var data = await employeeRepository.GetPositionLookupAsync(
            cancellationToken);

        return Result.Success(data);
    }
}