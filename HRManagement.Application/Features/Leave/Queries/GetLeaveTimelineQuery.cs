using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Application.Features.Leave.Queries
{
    public class GetLeaveTimelineQuery : IRequest<Result<ApiResponse<List<LeaveTimelineDto>>>>
    {
        public int LeaveId { get; set; }

        public GetLeaveTimelineQuery(int leaveId)
        {
            LeaveId = leaveId;
        }
    }

    internal class GetLeaveTimelineQueryHandler
        : IRequestHandler<GetLeaveTimelineQuery, Result<ApiResponse<List<LeaveTimelineDto>>>>
    {
        private readonly ILogger<GetLeaveTimelineQuery> _logger;
        private readonly ILeaveRepository _repo;

        public GetLeaveTimelineQueryHandler(
            ILeaveRepository repo,
            ILogger<GetLeaveTimelineQuery> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse<List<LeaveTimelineDto>>>> Handle(
            GetLeaveTimelineQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _repo.GetLeaveTimeline(request.LeaveId);

                return ApiHelperResponse.Success(
                    "Read leave timeline successfully",
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get leave timeline.");

                return ApiHelperResponse.Failed<List<LeaveTimelineDto>>(
                    "Failed to get leave timeline");
            }
        }
    }
}