using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveResponse;
using MediatR;

namespace HRManagement.Api.Application.Commands.LeaveManagementCommands
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
                var readResult = await _repo.getLeaveRequestById(request.LeaveRequestDto.InitialRequestId ?? -1);
                if (readResult == null) return ApiHelperResponse.Failed("request with {request.LeaveRequestDto.InitialRequestId} initial id not found");
                
                var history = mapToHistory(readResult);
                var createResult = await _repo.createLeaveRequestHistory(history);
                if (!createResult ) return ApiHelperResponse.Failed("failed create history");


                var updateResult = await _repo.updateLeaveRequest(mapFromUpdateDto(request.LeaveRequestDto, readResult));

                return ApiHelperResponse.Success("success created update");
               
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ApiHelperResponse.Failed("failed to update request");
            }
        }

        private LeaveRequestModel mapFromUpdateDto(UpdateLeaveRequestDto dto, LeaveRequestModel prev)
        {
            return new LeaveRequestModel
            {
                RequesterId = prev.RequesterId,
                SupervisorId = prev.SupervisorId,

                LeaveStartDate = dto.LeaveStartDate ?? prev.LeaveStartDate,
                LeaveStatus = dto.LeaveStatus ?? prev.LeaveStatus,
                LeaveDescription = dto.LeaveDescription ?? prev.LeaveDescription,
                DayAmount = dto.DayAmount ?? prev.DayAmount,
                LeaveType = dto.LeaveType ?? prev.LeaveType,
                AttachmentPath = dto.AttachmentPath ?? prev.AttachmentPath,

                IsCompleted = dto.LeaveStatus == 2 ? 1 : 0,

                CreatedBy = prev.CreatedBy,
                CreatedUtcDate = prev.CreatedUtcDate,

                ModifiedBy = dto.IsSupervisor ? prev.SupervisorId : prev.RequesterId,
                ModifiedUtcDate = DateTime.UtcNow

            };
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
                AttachmentPath = dto.AttachmentPath,

                IsCompleted = dto.LeaveStatus == 2 ? 1 : 0,
                InitialRequestId = dto.LeaveId,

                CreatedBy = dto.CreatedBy,
                CreatedUtcDate = dto.CreatedUtcDate,

                ModifiedBy = dto.RequesterId,
                ModifiedUtcDate = DateTime.UtcNow

            };
        }
    }
}
