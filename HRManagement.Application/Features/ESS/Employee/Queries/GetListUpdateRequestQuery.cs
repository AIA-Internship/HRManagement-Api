using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetListUpdateRequestQuery(int? Status) : IRequest<Result<List<EmployeeRequestResponseDto>>>;

internal sealed class GetListUpdateRequestQueryHandler(
    IRequestRepository requestRepository,
    ILogger<GetListUpdateRequestQueryHandler> logger) : IRequestHandler<GetListUpdateRequestQuery, Result<List<EmployeeRequestResponseDto>>>
{
    public async Task<Result<List<EmployeeRequestResponseDto>>> Handle(GetListUpdateRequestQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetListUpdateRequestQueryHandler));

        var data = await requestRepository.GetMyEmployeeUpdateRequestAsync(request.Status, null, cancellationToken);

        return Result.Success(data);
    }
}