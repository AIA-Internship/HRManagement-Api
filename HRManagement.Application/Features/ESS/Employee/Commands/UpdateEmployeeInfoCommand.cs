using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.SeedWork;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Commands;

public record UpdateEmployeeInfoCommand(string EmployeeDisplayId, UpdateEmploymentInfoPayload Payload, int CurrentUserId) : IRequest<Result>;

internal sealed class UpdateEmployeeInfoCommandHandler(
    IEmployeeRepository employeeRepository,
    ILogger<UpdateEmployeeInfoCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEmployeeInfoCommand, Result>
{
    public async Task<Result> Handle(UpdateEmployeeInfoCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(UpdateEmployeeInfoCommandHandler));

        var payload = request.Payload;
        var currentUserId = request.CurrentUserId;

        int? supervisorId = null;
        if (!string.IsNullOrWhiteSpace(payload.SupervisorDisplayId))
        {
            var supervisor = await employeeRepository.GetProfileByDisplayIdAsync(payload.SupervisorDisplayId);
            supervisorId = supervisor?.Id;
        }

        var empInfo = await employeeRepository.GetEmploymentInformationByDisplayIdAsync(request.EmployeeDisplayId);
        if (empInfo is null) return Result.Failure("Data tidak ditemukan.");

        empInfo.UpdateDetails(
            payload.EmploymentStatus,
            payload.StartDate,
            payload.EmploymentType,
            payload.Department,
            payload.Position,
            supervisorId,
            payload.EmployeeDisplayId,
            currentUserId
        );

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
