using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.SeedWork;
using MediatR;

namespace HRManagement.Api.Application.Queries.LeaveManagementQueries
{
    public class GetLeaveRequestBySupervisorId(int requsterId, int max): IRequest<Result<ApiResponse>>
    {
        public int RequesterId { get; set; } = requsterId;
        public int Max { get; set; } = max;


    }

    internal class GetLeaveRequestBySupervisorIdHandler : IRequestHandler<GetLeaveRequestBySupervisorId, Result<ApiResponse>>
    {
        private readonly ILogger<GetLeaveRequestBySupervisorIdHandler> _logger;
        private readonly ILeaveRepository _repo;

        public GetLeaveRequestBySupervisorIdHandler(
            ILeaveRepository repo
            , ILogger<GetLeaveRequestBySupervisorIdHandler> logger
            , IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(GetLeaveRequestBySupervisorId request,
            CancellationToken cancellationToken)
        {
            _logger.LogTrace("Executing handler for request : {request}",
                nameof(GetLeaveRequestByRequesterQueryHandler));

            try
            {
                var entity = await _repo.getLeaveRequestBySupervisorId(request.RequesterId, request.Max);

                if (entity == null) return ApiHelperResponse.Failed("data not found in system");


                List<ReadLeaveRequestDto> data = new List<ReadLeaveRequestDto>();

                foreach (var d in entity)
                {
                    data.Add(mapToReadDto(d));
                }


                return ApiHelperResponse.Success("data retrieved successfully", data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ApiHelperResponse.Failed("Failed to read leave request");
            }
        }

        public ReadLeaveRequestDto mapToReadDto(LeaveRequestModel model)
        {
            DateTime endDate = model.LeaveStartDate;

            if (model.DayAmount > 0.5m)
            {
                int remainingDays = (int)model.DayAmount - 1;

                while (remainingDays > 0)
                {
                    endDate = endDate.AddDays(1);

                    if (endDate.DayOfWeek != DayOfWeek.Saturday &&
                        endDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        remainingDays--;
                    }
                }
            }

            return new ReadLeaveRequestDto
            {
                leaveId = model.LeaveId,
                requesterId = model.RequesterId,
                supervisorId = model.SupervisorId,
                leaveDescription = model.LeaveDescription,
                leaveStatus = model.LeaveStatus.ToString(),
                leaveStartDate = model.LeaveStartDate,
                endDate = endDate,
                dayAmount = model.DayAmount,
                leaveType = (model.LeaveType ?? 0).ToString(),
                isCompleted = model.IsCompleted == 0 ? false : true,
                attachmentPath = model.AttachmentPath != null
                    ? MappingHelper.splitAttachmentPath(model.AttachmentPath)
                    : null,
                createdUtcDate = model.CreatedUtcDate
            };
        }
    }
}