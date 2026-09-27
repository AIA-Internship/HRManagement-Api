using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetPositionNamesLookupQuery : IRequest<Result<List<string>>>;

internal sealed class GetPositionNamesLookupQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<GetPositionNamesLookupQueryHandler> logger)
    : IRequestHandler<GetPositionNamesLookupQuery, Result<List<string>>>
{
    public async Task<Result<List<string>>> Handle(
        GetPositionNamesLookupQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetPositionNamesLookupQueryHandler));
        var positions = await employeeRepository.GetDistinctEmployeePositionNamesAsync(cancellationToken);
        return Result.Success(positions);
    }
}