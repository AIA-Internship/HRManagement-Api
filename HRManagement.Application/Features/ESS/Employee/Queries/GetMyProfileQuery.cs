using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetMyProfileQuery(string CurrentUserEmail) : IRequest<Result<EmployeeProfileResponseDto>>;

internal sealed class GetMyProfileQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<GetMyProfileQueryHandler> logger) : IRequestHandler<GetMyProfileQuery, Result<EmployeeProfileResponseDto>>
{
    public async Task<Result<EmployeeProfileResponseDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetMyProfileQueryHandler));

        var data = await employeeRepository.GetByEmailAsync(request.CurrentUserEmail, cancellationToken);

        return Result.Success(data);
    }
}
