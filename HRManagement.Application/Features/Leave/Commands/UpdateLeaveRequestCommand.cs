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
                var readResult = await _repo.getLeaveRequestById(request.LeaveRequestDto.InitialRequestId ?? -1, request.LeaveRequestDto.RequestId ?? -1);
                if (readResult == null) return ApiHelperResponse.Failed("request with {request.LeaveRequestDto.InitialRequestId} initial id not found");
                
                var history = mapToHistory(readResult);
                var createResult = await _repo.createLeaveRequestHistory(history);
                if (!createResult ) return ApiHelperResponse.Failed("failed create history");


                var updateResult = await _repo.updateLeaveRequest(mapFromUpdateDto(request.LeaveRequestDto, readResult));

                return ApiHelperResponse.Success();
               
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ApiHelperResponse.Failed("failed to update request");
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

                RequesterId = dto.RequesterId,
                SupervisorId = dto.SupervisorId,
                LeaveStartDate = dto.LeaveStartDate,
                LeaveStatus = dto.LeaveStatus ,
                LeaveDescription = dto.LeaveDescription ,
                DayAmount = dto.DayAmount ,
                LeaveType = dto.LeaveType ,

                IsCompleted = dto.LeaveStatus == 2 ? 1 : 0,
                InitialRequestId = dto.LeaveId,

                CreatedBy = dto.RequesterId,
                CreatedUtcDate = dto.CreatedUtcDate,

                ModifiedBy = dto.RequesterId,
                ModifiedUtcDate = DateTime.UtcNow,
                SupervisorComment = dto.SupervisorComment
            };
        }
    }
}
