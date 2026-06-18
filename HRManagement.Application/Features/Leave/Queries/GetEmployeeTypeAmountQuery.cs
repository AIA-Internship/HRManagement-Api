using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveBalance;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using MediatR;

namespace HRManagement.Api.Application.Queries.LeaveManagementQueries
{
    public class getEmployeeTypeAmountQuery : IRequest<Result<ApiResponse>>
    {
        public int RequesterId { get; set; }

        public getEmployeeTypeAmountQuery(int requesterid)
        {
            RequesterId = requesterid;
        }
    }

    internal class getEmployeeTypeAmountQueryHandler : IRequestHandler<getEmployeeTypeAmountQuery, Result<ApiResponse>>
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
        public async Task<Result<ApiResponse>> Handle(getEmployeeTypeAmountQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var res = await _repo.GetLeaveTypeCounts(request.RequesterId);

                if (res == null)
                    return ApiHelperResponse.Failed("data not found in system");

                return ApiHelperResponse.Success("data retrieved successfully", res);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ApiHelperResponse.Failed("Failed to read leave type amount");
            }
        }
    }

}
