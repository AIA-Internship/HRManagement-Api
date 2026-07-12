using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.Leave.Commands;

public record UploadLeaveAttachmentCommand(int LeaveId, string DocumentType, List<UploadFileResponseDto> Files, int CurrentUserId) : IRequest<Result>;

internal sealed class UploadLeaveAttachmentCommandHandler(
    ILeaveRepository leaveRepository,
    ILogger<UploadLeaveAttachmentCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadLeaveAttachmentCommand, Result>
{
    public async Task<Result> Handle(UploadLeaveAttachmentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(UploadLeaveAttachmentCommandHandler));

        var attachments = new List<LeaveAttachment>();
        foreach (var item in request.Files)
        {
            attachments.Add(new LeaveAttachment
            (
                request.LeaveId,
                request.DocumentType,
                item.FileName,
                item.FileUrl,
                item.ContentType,
                item.FileSize,
                request.CurrentUserId
            ));
        }

        await leaveRepository.AddLeaveAttachmentsAsync(attachments, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
