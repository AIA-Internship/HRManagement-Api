using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces.LeaveManagementInterface;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HRManagement.Api.Application.Queries.LeaveManagementQueries
{
    public class GetLeaveRequestByRequesterQuery(int requsterId, int max): IRequest<Result<ApiResponse>>
    {
        public int RequesterId { get; set; } = requsterId;
        public int Max { get; set; } = max;


    }
    internal class GetLeaveRequestByRequesterQueryHandler: IRequestHandler<GetLeaveRequestByRequesterQuery, Result<ApiResponse>>
    {
        private readonly ILogger<GetLeaveRequestByRequesterQueryHandler> _logger;
        private readonly ILeaveRequestRepository _repo;

        public GetLeaveRequestByRequesterQueryHandler(
            ILeaveRequestRepository repo
            , ILogger<GetLeaveRequestByRequesterQueryHandler> logger
            , IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _logger = logger;
        }
        public async Task<Result<ApiResponse>> Handle(GetLeaveRequestByRequesterQuery request, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetLeaveRequestByRequesterQueryHandler));

            try
            {
                var entity = await _repo.getLeaveRequestsByRequesterId(request.RequesterId, request.Max);

                if (entity == null) return ApiHelperResponse.Failed("data not found in system");


                List<ReadLeaveRequestDto> data = new List<ReadLeaveRequestDto>();

                foreach(var d in entity)
                {
                    data.Add(mapToReadDto(d));
                }


                return ApiHelperResponse.Success("data retrieved successfully", data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
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
