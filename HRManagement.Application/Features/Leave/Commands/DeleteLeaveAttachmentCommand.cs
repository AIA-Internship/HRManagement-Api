using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.SeedWork;
using MediatR;

namespace HRManagement.Application.Features.Leave.Commands
{
    public class DeleteLeaveAttachmentCommand : IRequest<Result>
    {
        public int AttachmentId { get; set; }
        public int RequesterId { get; set; }

        public DeleteLeaveAttachmentCommand(int attachmentId, int requesterId)
        {
            AttachmentId = attachmentId;
            RequesterId = requesterId;
        }
    }

    internal class DeleteLeaveAttachmentCommandHandler
        : IRequestHandler<DeleteLeaveAttachmentCommand, Result>
    {
        private readonly ILeaveRepository _repo;
        private readonly ILogger<DeleteLeaveAttachmentCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteLeaveAttachmentCommandHandler(
            ILeaveRepository repo,
            ILogger<DeleteLeaveAttachmentCommandHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteLeaveAttachmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start {Handler}", nameof(DeleteLeaveAttachmentCommandHandler));

                var attachment = await _repo.GetAttachmentByIdAsync(request.AttachmentId);

                if (attachment == null)
                    return Result.Failure("Attachment not found.");

                await _repo.DeleteLeaveAttachmentByIdAsync(request.AttachmentId, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("End {Handler}", nameof(DeleteLeaveAttachmentCommandHandler));

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete attachments.");
                return Result.Failure("Failed to delete attachments.");
            }
        }
    }
}