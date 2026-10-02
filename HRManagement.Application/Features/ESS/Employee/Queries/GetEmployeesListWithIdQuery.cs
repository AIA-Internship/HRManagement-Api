using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetEmployeesListWithIdQuery: IRequest<Result<List<PerformanceReviewEmployeeDto>>>;

internal sealed class GetEmployeesListWithIdQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<GetEmployeesListWithIdQueryHandler> logger)
    : IRequestHandler<GetEmployeesListWithIdQuery, Result<List<PerformanceReviewEmployeeDto>>>
{
    public async Task<Result<List<PerformanceReviewEmployeeDto>>> Handle(
        GetEmployeesListWithIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler : {HandlerName}",
            nameof(GetEmployeesListWithIdQueryHandler));

        var data = await employeeRepository.GetPerformanceReviewEmployeesAsync(
            cancellationToken);

        return Result.Success(data);
    }
}