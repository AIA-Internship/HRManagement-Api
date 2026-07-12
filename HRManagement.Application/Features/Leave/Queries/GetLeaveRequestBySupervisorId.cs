using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using MediatR;

namespace HRManagement.Application.Features.Leave.Queries
{
    public class GetLeaveRequestBySupervisorId(int supervisorId, int max): IRequest<Result<ApiResponse<List<ReadLeaveRequestDto>>>>
    {
        public int SupervisorId { get; set; } = supervisorId;
        public int Max { get; set; } = max;


    }

    internal class GetLeaveRequestBySupervisorIdHandler : IRequestHandler<GetLeaveRequestBySupervisorId, Result<ApiResponse<List<ReadLeaveRequestDto>>>>
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

        public async Task<Result<ApiResponse<List<ReadLeaveRequestDto>>>> Handle(GetLeaveRequestBySupervisorId request,
            CancellationToken cancellationToken)
        {
            _logger.LogTrace("Executing handler for request : {request}",
                nameof(GetLeaveRequestByRequesterQueryHandler));

            try
            {
                var entity = await _repo.getLeaveRequestBySupervisorId(request.SupervisorId, request.Max);

                if (entity == null) return ApiHelperResponse.Failed<List<ReadLeaveRequestDto>>("data not found in system");


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
                return ApiHelperResponse.Failed<List<ReadLeaveRequestDto>>("Failed to read leave request");
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
                createdUtcDate = model.CreatedUtcDate
            };
        }
    }
}