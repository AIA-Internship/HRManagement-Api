using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetMyUpdateRequestQuery(int Id, int? Status) : IRequest<Result<List<EmployeeRequestResponseDto>>>;

internal sealed class GetMyUpdateRequestQueryHandler(
    IRequestRepository requestRepository,
    ILogger<GetMyUpdateRequestQueryHandler> logger) : IRequestHandler<GetMyUpdateRequestQuery, Result<List<EmployeeRequestResponseDto>>>
{
    public async Task<Result<List<EmployeeRequestResponseDto>>> Handle(GetMyUpdateRequestQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetMyUpdateRequestQueryHandler));

        var data = await requestRepository.GetMyEmployeeUpdateRequestAsync(request.Status, request.Id, cancellationToken);

        return Result.Success(data);
    }
}