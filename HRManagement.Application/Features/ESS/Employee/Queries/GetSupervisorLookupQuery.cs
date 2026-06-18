using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetSupervisorLookupQuery() : IRequest<Result<List<SupervisorLookupResponseDto>>>;

internal sealed class GetSupervisorLookupQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<GetSupervisorLookupQueryHandler> logger) : IRequestHandler<GetSupervisorLookupQuery, Result<List<SupervisorLookupResponseDto>>>
{
    public async Task<Result<List<SupervisorLookupResponseDto>>> Handle(GetSupervisorLookupQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetSupervisorLookupQueryHandler));

        var data = await employeeRepository.GetSupervisorLookupAsync(cancellationToken);

        return Result.Success(data);
    }
}
