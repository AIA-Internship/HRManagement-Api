using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using MediatR;

namespace HRManagement.Application.Features.Leave.Queries
{
    public class getLeaveBalanceQuery: IRequest<Result<ApiResponse>>
    {
        public int RequesterId { get; set; }

        public getLeaveBalanceQuery(int requesterid)
        {
            RequesterId = requesterid;
        }

    }

    internal class getLeaveBalanceQueryHandler : IRequestHandler<getLeaveBalanceQuery, Result<ApiResponse>>
    {
        private readonly ILogger<getLeaveBalanceQueryHandler> _logger;
        private readonly ILeaveRepository _repo;

        public getLeaveBalanceQueryHandler(
            ILeaveRepository repo
            , ILogger<getLeaveBalanceQueryHandler> logger
            )
        {
            _repo = repo;
            _logger = logger;
        }
        public async Task<Result<ApiResponse>> Handle(getLeaveBalanceQuery request, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(getLeaveBalanceQueryHandler));

            try
            {
                var res = await _repo.getLeaveBalanceById(request.RequesterId);

                if (res == null) return ApiHelperResponse.Failed("data not found in system");

                return ApiHelperResponse.Success("data retrieved successfully", mapToReadDto(res));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ApiHelperResponse.Failed("Failed to read leave balance");
            }
        }

        public ReadLeaveBalanceDto mapToReadDto(LeaveBalanceModel model)
        {
            return new ReadLeaveBalanceDto
            {
                EmployeeId = model.EmployeeId,
                LeaveBalance = model.LeaveBalance
            };
        }
    }

}
