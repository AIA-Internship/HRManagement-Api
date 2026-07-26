using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Application.Features.Leave.Commands
{
    public class DeleteLeaveRequestCommand : IRequest<Result<ApiResponse>>
    {
        public DeleteLeaveRequestDto LeaveRequestDto { get; set; }
        public DeleteLeaveRequestCommand(DeleteLeaveRequestDto dto)
        {
            LeaveRequestDto = dto;
        }
    }
    internal class DeleteLeaveRequestCommandHandler : IRequestHandler<DeleteLeaveRequestCommand, Result<ApiResponse>>
    {
        private readonly ILogger<DeleteLeaveRequestCommandHandler> _logger;
        private readonly ILeaveRepository _repo;
        public DeleteLeaveRequestCommandHandler(
            ILeaveRepository repo
            , ILogger<DeleteLeaveRequestCommandHandler> logger
        )
        {
            _repo = repo;
            _logger = logger;
        }
        public async Task<Result<ApiResponse>> Handle(DeleteLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existing = await _repo.getLeaveRequestById(
                    request.LeaveRequestDto.RequestId,
                    request.LeaveRequestDto.RequesterId);

                if (existing == null)
                    return ApiHelperResponse.Failed("Leave request not found");

                var result = await _repo.DeleteLeaveRequest(existing.LeaveId);

                if (!result)
                    return ApiHelperResponse.Failed("Failed to delete leave request");

                return ApiHelperResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete leave request");
                return ApiHelperResponse.Failed("Failed to delete leave request");
            }
        }

    }
}