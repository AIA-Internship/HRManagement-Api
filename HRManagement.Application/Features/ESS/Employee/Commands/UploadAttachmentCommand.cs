using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Commands;

public record UploadAttachmentCommand(int Id, string DocumentType, List<UploadFileResponseDto> Files, int CurrentUserId) : IRequest<Result>;

internal sealed class UploadAttachmentCommandHandler(
    IEmployeeRepository employeeRepository,
    ILogger<UploadAttachmentCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadAttachmentCommand, Result>
{
    public async Task<Result> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(UploadAttachmentCommandHandler));

        var attachments = new List<EmployeeAttachment>();
        foreach (var item in request.Files)
        {
            attachments.Add(new EmployeeAttachment
            (
                request.Id,
                request.DocumentType,
                item.FileName,
                item.FileUrl,
                item.ContentType,
                item.FileSize,
                request.CurrentUserId
            ));
        }

        await employeeRepository.AddEmployeeAttachmentsAsync(attachments, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}