using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using MediatR;

namespace HRManagement.Application.Features.Leave.Queries
{
    public class GetLeaveAttachmentsQuery(int leaveId)
        : IRequest<Result<ApiResponse<List<ReadLeaveAttachmentDto>>>>
    {
        public int LeaveId { get; set; } = leaveId;
    }

    internal class GetLeaveAttachmentsQueryHandler
        : IRequestHandler<GetLeaveAttachmentsQuery, Result<ApiResponse<List<ReadLeaveAttachmentDto>>>>
    {
        private readonly ILogger<GetLeaveAttachmentsQueryHandler> _logger;
        private readonly ILeaveRepository _repo;

        public GetLeaveAttachmentsQueryHandler(
            ILeaveRepository repo,
            ILogger<GetLeaveAttachmentsQueryHandler> logger,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse<List<ReadLeaveAttachmentDto>>>> Handle(
            GetLeaveAttachmentsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogTrace(
                "Executing handler for request : {request}",
                nameof(GetLeaveAttachmentsQueryHandler));

            try
            {
                var entity = await _repo.getLeaveAttachmentsByLeaveId(request.LeaveId);

                if (entity == null)
                    return ApiHelperResponse.Failed<List<ReadLeaveAttachmentDto>>(
                        "Data not found in system");

                var data = entity
                    .Select(MapToReadDto)
                    .ToList();

                return ApiHelperResponse.Success(
                    "Data retrieved successfully",
                    data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to retrieve leave attachments");

                return ApiHelperResponse.Failed<List<ReadLeaveAttachmentDto>>(
                    "Failed to retrieve leave attachments");
            }
        }

        private ReadLeaveAttachmentDto MapToReadDto(
            LeaveAttachment model)
        {
            return new ReadLeaveAttachmentDto
            {
                AttachmentId = model.AttachmentId,
                LeaveId = model.LeaveId,
                DocumentType = model.DocumentType,
                FileName = model.FileName,
                FilePath = model.FilePath,
                ContentType = model.ContentType,
                FileSize = model.FileSize,
                IsActive = model.IsActive
            };
        }
    }
}