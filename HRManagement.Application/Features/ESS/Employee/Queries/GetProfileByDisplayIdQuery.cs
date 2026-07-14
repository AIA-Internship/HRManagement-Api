using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetProfileByDisplayIdQuery(string DisplayId) : IRequest<Result<EmployeeProfileResponseDto>>;

internal sealed class GetProfileByDisplayIdQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<GetProfileByDisplayIdQueryHandler> logger) : IRequestHandler<GetProfileByDisplayIdQuery, Result<EmployeeProfileResponseDto>>
{
    public async Task<Result<EmployeeProfileResponseDto>> Handle(GetProfileByDisplayIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetProfileByDisplayIdQueryHandler));

        var data = await employeeRepository.GetProfileByDisplayIdAsync(request.DisplayId, cancellationToken);
        if (data is null) return Result.Failure<EmployeeProfileResponseDto>("Data tidak ditemukan.");

        return Result.Success(data);
    }
}
