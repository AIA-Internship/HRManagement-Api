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
                nameof(GetLeaveRequestBySupervisorIdHandler));

            try
            {
                var entity = await _repo.getLeaveRequestBySupervisorId(request.SupervisorId, request.Max);

                if (entity == null || entity.Count == 0) return ApiHelperResponse.Success("data retrieved successfully", new List<ReadLeaveRequestDto>());


                List<ReadLeaveRequestDto> data = new List<ReadLeaveRequestDto>();

                foreach (var d in entity)
                {
                    try
                    {
                        data.Add(mapToReadDto(d));
                    }
                    catch (Exception mapEx)
                    {
                        _logger.LogError(mapEx, "Error mapping leave request {LeaveId}", d?.LeaveId);
                        Console.WriteLine($"Error mapping leaveId {d?.LeaveId}: {mapEx}");
                        // Continue with next record instead of failing completely
                    }
                }


                return ApiHelperResponse.Success("data retrieved successfully", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLeaveRequestBySupervisorIdHandler");
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
                requesterDisplayId  = model.RequesterDisplayId,
                supervisorId = model.SupervisorId,
                leaveDescription = model.LeaveDescription,
                leaveStatus = model.LeaveStatus.ToString(),
                leaveStartDate = model.LeaveStartDate,
                endDate = endDate,
                dayAmount = model.DayAmount,
                leaveType = (model.LeaveType ?? 0).ToString(),
                isCompleted = model.IsCompleted == 0 ? false : true,
                createdUtcDate = model.CreatedUtcDate,
                SupervisorComment = model.SupervisorComment
            };
        }
    }
}