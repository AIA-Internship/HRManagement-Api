using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.Master.References.Queries;

public record GetLookupValuesQuery(string Category) : IRequest<Result<List<LookupResponseDto>>>;

internal sealed class GetLookupValuesQueryHandler(
    ILookupRepository lookupRepository,
    ILogger<GetLookupValuesQueryHandler> logger) : IRequestHandler<GetLookupValuesQuery, Result<List<LookupResponseDto>>>
{
    public async Task<Result<List<LookupResponseDto>>> Handle(GetLookupValuesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetLookupValuesQueryHandler));

        var data = await lookupRepository.GetLookupListAsync(request.Category, cancellationToken);

        return Result.Success(data);
    }
}