using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using MediatR;

namespace HRManagement.Application.Features.Leave.Queries
{
    public class GetLeaveRequestByIdQuery : IRequest<Result<ApiResponse<ReadLeaveRequestDto>>>
    {
        public int RequestId { get; set; }

        public int RequesterId { get; set; }

        public GetLeaveRequestByIdQuery(int requestId, int requesterId)
        {
            RequestId = requestId;
            RequesterId = requesterId;
        }
    }
    internal class GetLeaveRequestByIdQueryHandler : IRequestHandler<GetLeaveRequestByIdQuery, Result<ApiResponse<ReadLeaveRequestDto>>>
    {
        private readonly ILogger<GetLeaveRequestByIdQuery> _logger;
        private readonly ILeaveRepository _repo;
        public GetLeaveRequestByIdQueryHandler(
            ILeaveRepository repo
            , ILogger<GetLeaveRequestByIdQuery> logger
            )
        {
            _repo = repo;
            _logger = logger;
        }
        public async Task<Result<ApiResponse<ReadLeaveRequestDto>>> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _repo.getLeaveRequestById(request.RequestId, request.RequesterId);
                if (result == null) return ApiHelperResponse.Failed<ReadLeaveRequestDto>("Leave request not found");


                return ApiHelperResponse.Success("read leave request successfully", mapToReadDto(result));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ApiHelperResponse.Failed<ReadLeaveRequestDto>("Failed to get leave request");
            }
        }

        public ReadLeaveRequestDto mapToReadDto(LeaveRequestModel model)
        {
            // Ensure RequesterDisplayId is never null/empty by using fallback logic
            var displayId = model.RequesterDisplayId;
            if (string.IsNullOrWhiteSpace(displayId))
            {
                // If not in the leave request model, it will need to be populated from employee data
                // This is handled at read time in repository
                displayId = "N/A";
            }

            return new ReadLeaveRequestDto
            {
                leaveId = model.LeaveId,
                requesterDisplayId = displayId,
                requesterId = model.RequesterId,
                supervisorId = model.SupervisorId,
                leaveDescription = model.LeaveDescription,
                leaveStatus = MappingHelper.leaveStatusFromInt(model.LeaveStatus).ToString(),
                leaveStartDate = model.LeaveStartDate,
                dayAmount = model.DayAmount,
                leaveType = MappingHelper.leaveTypeFromInt(model.LeaveType ?? 0).ToString(),
                isCompleted = model.IsCompleted == 0 ? false : true,
                createdUtcDate = model.CreatedUtcDate
            };
        }
    }
}
