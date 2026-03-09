using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces.LeaveManagementInterface;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
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
        private readonly ILeaveRequestRepository _repo;
        public UpdateLeaveRequestCommandHandler(
            ILeaveRequestRepository repo
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
                
                var updateResult = await _repo.createLeaveRequest(mapFromUpdateDto(request.LeaveRequestDto, readResult));

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

                IsEdit = 1,
                IsCompleted = dto.LeaveStatus == 2 ? 1 : 0,
                InitialRequestId = prev.InitialRequestId == -1 ? prev.LeaveId : prev.InitialRequestId,

                CreatedBy = prev.CreatedBy,
                CreatedUtcDate = prev.CreatedUtcDate,

                ModifiedBy = dto.IsSupervisor ? prev.SupervisorId : prev.RequesterId,
                ModifiedUtcDate = DateTime.UtcNow

            };
        }
    }
}
