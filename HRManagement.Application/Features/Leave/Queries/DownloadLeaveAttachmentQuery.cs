using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.SeedWork;
using MediatR;

namespace HRManagement.Application.Features.Leave.Queries
{
    public class DownloadLeaveAttachmentQuery(
        int attachmentId,
        int requesterId)
        : IRequest<Result<DownloadLeaveAttachmentResult>>
    {
        public int AttachmentId { get; set; } = attachmentId;
        public int RequesterId { get; set; } = requesterId;
    }

    public class DownloadLeaveAttachmentResult
    {
        public Stream Content { get; set; } = Stream.Null;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }

    internal class DownloadLeaveAttachmentQueryHandler
        : IRequestHandler<
            DownloadLeaveAttachmentQuery,
            Result<DownloadLeaveAttachmentResult>>
    {
        private readonly ILogger<DownloadLeaveAttachmentQueryHandler> _logger;
        private readonly ILeaveRepository _repo;
        private readonly IFileStorageService _fileStorageService;

        public DownloadLeaveAttachmentQueryHandler(
            ILeaveRepository repo,
            IFileStorageService fileStorageService,
            ILogger<DownloadLeaveAttachmentQueryHandler> logger)
        {
            _repo = repo;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<Result<DownloadLeaveAttachmentResult>> Handle(
            DownloadLeaveAttachmentQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogTrace(
                "Executing handler for request : {request}",
                nameof(DownloadLeaveAttachmentQueryHandler));

            try
            {
                var attachment = await _repo.GetAttachmentByIdAsync(
                    request.AttachmentId);

                if (attachment == null)
                {
                    return Result.Failure<DownloadLeaveAttachmentResult>(
                        "Attachment not found.");
                }

                // TODO:
                // Validate that this attachment belongs
                // to the requester.

                if (string.IsNullOrWhiteSpace(attachment.FilePath))
                {
                    return Result.Failure<DownloadLeaveAttachmentResult>(
                        "Attachment file path is empty.");
                }

                var fileName = Path.GetFileName(attachment.FilePath);

                var downloadResult = await _fileStorageService.DownloadBlobAsync(
                    fileName,
                    cancellationToken);

                if (downloadResult == null)
                {
                    return Result.Failure<DownloadLeaveAttachmentResult>(
                        "Attachment content not found in storage.");
                }

                // Convert downloaded byte[] to a readable stream for the controller
                var stream = new MemoryStream(downloadResult.Content ?? Array.Empty<byte>());

                return Result.Success(
                    new DownloadLeaveAttachmentResult
                    {
                        Content = stream,
                        FileName = attachment.FileName,
                        ContentType = downloadResult.ContentType ?? attachment.ContentType
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to download leave attachment.");

                return Result.Failure<DownloadLeaveAttachmentResult>(
                    "Failed to download leave attachment.");
            }
        }

        private static string GetBlobName(string filePath)
        {
            if (Uri.TryCreate(filePath, UriKind.Absolute, out var uri))
            {
                return uri.AbsolutePath.TrimStart('/');
            }

            return filePath.TrimStart('/');
        }
    }
}