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
                var existing = await _repo.getLeaveRequestById(request.LeaveRequestDto.RequestId);

                if (existing == null)
                    return ApiHelperResponse.Failed("Leave request not found");

                var result = await _repo.DeleteLeaveRequest(existing.LeaveId);

                if (!result)
                    return ApiHelperResponse.Failed("Failed to delete leave request");

                try
                {
                    if (existing.LeaveStatus == 2) 
                    {
                        var balance = await _repo.getLeaveBalanceById(existing.RequesterId);
                        if (balance != null)
                        {
                            balance.LeaveBalance += existing.DayAmount;
                            await _repo.updateLeaveBalance(balance);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log and continue; deletion already succeeded but balance restore failed
                    _logger.LogError(ex, "Failed to restore leave balance after deleting leave request {LeaveId}", existing.LeaveId);
                }

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