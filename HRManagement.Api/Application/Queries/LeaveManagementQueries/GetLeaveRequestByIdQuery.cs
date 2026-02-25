using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces.NewFolder;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
using MediatR;

namespace HRManagement.Api.Application.Queries.LeaveManagementQueries
{
    public class GetLeaveRequestByIdQuery : IRequest<Result<ApiResponse>>
     {
        public int RequestId { get; set; }
        public GetLeaveRequestByIdQuery(int requestId)
        {
            RequestId = requestId;
        }
    }
    internal class GetLeaveRequestByIdQueryHandler : IRequestHandler<GetLeaveRequestByIdQuery, Result<ApiResponse>>
    {
        private readonly ILogger<GetLeaveRequestByIdQuery> _logger;
        private readonly ILeaveRequestRepository _repo;
        public GetLeaveRequestByIdQueryHandler(
            ILeaveRequestRepository repo
            , ILogger<GetLeaveRequestByIdQuery> logger
            )
        {
            _repo = repo;
            _logger = logger;
        }
        public async Task<Result<ApiResponse>> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _repo.getLeaveRequestById(request.RequestId);
                if(result == null) return ApiHelperResponse.Failed("Leave request not found");


                return ApiHelperResponse.Success("read leave request successfully", mapToReadDto(result));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ApiHelperResponse.Failed("Failed to delete leave request");
            }
        }

        public ReadLeaveRequestDto mapToReadDto(LeaveRequestModel model)
        {

            return new ReadLeaveRequestDto
            {
                leaveId = model.LeaveId,
                requesterId = model.RequesterId,
                supervisorId = model.SupervisorId,
                leaveDescription = model.LeaveDescription,
                leaveStatus = MappingHelper.leaveStatusFromInt(model.LeaveStatus).ToString(),
                leaveStartDate = model.LeaveStartDate,
                dayAmount = model.DayAmount,
                leaveType = MappingHelper.leaveTypeFromInt(model.LeaveType ?? 0).ToString(),
                isCompleted = model.IsCompleted == 0 ? false : true,
                isEdit = model.IsEdit == 0 ? false : true,
                initialRequestId = model.InitialRequestId,
                attachmentPath = model.AttachmentPath != null
                    ? MappingHelper.splitAttachmentPath(model.AttachmentPath)
                    : null,
                createdUtcDate = model.CreatedUtcDate
            };
        }
    }
}
