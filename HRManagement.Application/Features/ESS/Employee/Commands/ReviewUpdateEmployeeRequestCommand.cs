using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.SeedWork;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Commands;

public record ReviewUpdateEmployeeRequestCommand(ReviewUpdatePayload Payload, int CurrentUserId) : IRequest<Result>;

internal sealed class ReviewUpdateEmployeeRequestCommandHandler(
    IRequestRepository requestRepository,
    ILogger<ReviewUpdateEmployeeRequestCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<ReviewUpdateEmployeeRequestCommand, Result>
{
    public async Task<Result> Handle(ReviewUpdateEmployeeRequestCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(ReviewUpdateEmployeeRequestCommandHandler));

        var payload = request.Payload;

        var dataRequest = await requestRepository.GetByIdWithEmployeeAsync(payload.RequestId, cancellationToken);
        if (dataRequest is null) return Result.Failure("Data tidak ditemukan.");
        if (dataRequest.Status != 0) return Result.Failure("Data tidak ditemukan.");

        if (payload.IsApproved)
        {
            dataRequest.Approve(payload.Reason, request.CurrentUserId);
            dataRequest.Employee.ApplyUpdate(dataRequest, request.CurrentUserId);
        }
        else
        {
            dataRequest.Reject(payload.Reason, request.CurrentUserId);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}