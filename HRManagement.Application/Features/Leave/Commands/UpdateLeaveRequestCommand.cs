using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using MediatR;

namespace HRManagement.Application.Features.Leave.Commands
{
    public class UpdateLeaveRequestCommand : IRequest<Result<ApiResponse>>
    {
        public UpdateLeaveRequestDto LeaveRequestDto { get; set; }
        public UpdateLeaveRequestCommand(UpdateLeaveRequestDto leaveRequestDto)
        {
            LeaveRequestDto = leaveRequestDto;
        }
    }

    internal class UpdateLeaveRequestCommandHandler : IRequestHandler<UpdateLeaveRequestCommand, Result<ApiResponse>>
    {
        private readonly ILogger<UpdateLeaveRequestCommandHandler> _logger;
        private readonly ILeaveRepository _repo;
        public UpdateLeaveRequestCommandHandler(
            ILeaveRepository repo
            , ILogger<UpdateLeaveRequestCommandHandler> logger
        )
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(UpdateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var readResult = await _repo.getLeaveRequestById(request.LeaveRequestDto.LeaveId);
                if (readResult == null)
                    return ApiHelperResponse.Failed($"Leave request with id {request.LeaveRequestDto.LeaveId} not found");

                var history = mapToHistory(readResult);
                var createResult = await _repo.createLeaveRequestHistory(history);
                if (!createResult ) return ApiHelperResponse.Failed("failed create history");


                var updateResult = await _repo.updateLeaveRequest(mapFromUpdateDto(request.LeaveRequestDto, readResult));

                return ApiHelperResponse.Success();
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());

                return ApiHelperResponse.Failed(ex.ToString());
            }
        }

        private LeaveRequestModel mapFromUpdateDto(UpdateLeaveRequestDto dto, LeaveRequestModel prev)
        {
            prev.LeaveStartDate = dto.LeaveStartDate ?? prev.LeaveStartDate;
            prev.LeaveStatus = dto.LeaveStatus ?? prev.LeaveStatus;
            prev.LeaveDescription = dto.LeaveDescription ?? prev.LeaveDescription;
            prev.DayAmount = dto.DayAmount ?? prev.DayAmount;
            prev.LeaveType = dto.LeaveType ?? prev.LeaveType;

            prev.IsCompleted = dto.LeaveStatus == 2 ? 1 : 0;

            prev.ModifiedUtcDate = DateTime.UtcNow;

            return prev;
        }

        private LeaveRequestHistory mapToHistory(LeaveRequestModel dto)
        {
            return new LeaveRequestHistory
            {
                LeaveId = dto.LeaveId,
                ModifiedUtcDate = DateTime.UtcNow
            };
        }
    }
}
