using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Application.Features.Leave.Queries
{
    public class getEmployeeTypeAmountQuery : IRequest<Result<ApiResponse<LeaveTypeCountDto>>>
    {
        public int RequesterId { get; set; }

        public getEmployeeTypeAmountQuery(int requesterId)
        {
            RequesterId = requesterId;
        }
    }

    internal class getEmployeeTypeAmountQueryHandler : IRequestHandler<getEmployeeTypeAmountQuery, Result<ApiResponse<LeaveTypeCountDto>>>
    {
        private readonly ILogger<getEmployeeTypeAmountQueryHandler> _logger;
        private readonly ILeaveRepository _repo;

        public getEmployeeTypeAmountQueryHandler(
            ILeaveRepository repo
            , ILogger<getEmployeeTypeAmountQueryHandler> logger
            )
        {
            _repo = repo;
            _logger = logger;
        }
        public async Task<Result<ApiResponse<LeaveTypeCountDto>>> Handle(getEmployeeTypeAmountQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var res = await _repo.GetLeaveTypeCounts(request.RequesterId);

                if (res == null)
                    return ApiHelperResponse.Failed<LeaveTypeCountDto>("data not found in system");

                return ApiHelperResponse.Success("data retrieved successfully", res);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ApiHelperResponse.Failed<LeaveTypeCountDto>("Failed to read leave type amount", LeaveTypeCountDto.empty());
            }
        }
    }

}
