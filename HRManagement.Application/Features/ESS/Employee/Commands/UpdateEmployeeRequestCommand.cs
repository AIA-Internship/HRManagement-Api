using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Commands;

public record UpdateEmployeeRequestCommand(UpdateEmployeePayload Payload, string CurrentUserEmail, int CurrentUserId) : IRequest<Result>;

internal sealed class UpdateEmployeeRequestCommandHandler(
    IEmployeeRepository employeeRepository,
    ILogger<UpdateEmployeeRequestCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEmployeeRequestCommand, Result>
{
    public async Task<Result> Handle(UpdateEmployeeRequestCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(UpdateEmployeeRequestCommandHandler));

        var payload = request.Payload;

        var employee = await employeeRepository.GetProfileByEmailAsync(request.CurrentUserEmail);
        if (employee is null) return Result.Failure("Data tidak ditemukan.");

        var obj = new EmployeeUpdateRequest(employee!, payload, request.CurrentUserId);
        await employeeRepository.AddEmployeeUpdateRequestAsync(obj, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}