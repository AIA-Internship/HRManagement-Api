using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces.LeaveManagementInterface;
using HRManagement.Api.Application.Queries;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.SeedWork;
using MediatR;

namespace HRManagement.Api.Application.Commands.LeaveManagementCommands
{
    public class CreateLeaveRequestCommand : IRequest<Result<ApiResponse>>
    {
        public CreateLeaveRequestDto LeaveRequestDto { get; set; }
        public CreateLeaveRequestCommand(CreateLeaveRequestDto leaveRequestDto)
        {
            LeaveRequestDto = leaveRequestDto;
        }
    }
    
    internal class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, Result<ApiResponse>>
    {
        private readonly ILogger<CreateLeaveRequestCommandHandler> _logger;
        private readonly ILeaveRequestRepository _repo;

        public CreateLeaveRequestCommandHandler(
            ILeaveRequestRepository repo
            , ILogger<CreateLeaveRequestCommandHandler> logger
        )
        {
            _repo = repo;
            _logger = logger;
        }
        public async Task<Result<ApiResponse>> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CreateLeaveRequestCommandHandler));

            try
            {
                bool created = await _repo.createLeaveRequest(mapFromCreateDto(request.LeaveRequestDto));

                if(!created) return ApiHelperResponse.Failed("Failed to create leave request");
                
                return ApiHelperResponse.Success("Leave request created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static LeaveRequestModel mapFromCreateDto(CreateLeaveRequestDto dto)
        {

            return new LeaveRequestModel
            {
                RequesterId = dto.RequesterId,
                SupervisorId = dto.SupervisorId,
                LeaveDescription = dto.LeaveDescription,
                LeaveStartDate = dto.leaveStartDate,
                DayAmount = dto.DayAmount,
                LeaveType = dto.LeaveType,
                AttachmentPath = MappingHelper.joinAttachmentPath(dto.AttachmentPath),
                IsDeleted = 0,
                IsCompleted = 0,
                IsEdit = 0,
                InitialRequestId = -1,
                CreatedBy = dto.RequesterId,
                CreatedUtcDate = DateTime.UtcNow,
                ModifiedBy = dto.RequesterId,
                ModifiedUtcDate = DateTime.UtcNow
            };

        }
    }
}
