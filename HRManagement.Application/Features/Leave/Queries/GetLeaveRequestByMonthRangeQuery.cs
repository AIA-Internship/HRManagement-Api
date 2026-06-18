using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using MediatR;

namespace HRManagement.Api.Application.Queries.LeaveManagementQueries
{
    public class GetLeaveRequestByMonthRangeQuery : IRequest<Result<ApiResponse>>
    {
        public int month;
        public int year;
        public GetLeaveRequestByMonthRangeQuery(int Year, int Month)
        {
            year = Year;
            month = Month;

        }

        public class Handler : IRequestHandler<GetLeaveRequestByMonthRangeQuery, Result<ApiResponse>>
        {

            private readonly ILogger<GetLeaveRequestByMonthRangeQuery> _logger;
            private readonly ILeaveRepository _repo;
            public Handler(
                ILeaveRepository repo
                , ILogger<GetLeaveRequestByMonthRangeQuery> logger
                )
            {
                _repo = repo;
                _logger = logger;
            }
            public async Task<Result<ApiResponse>> Handle(GetLeaveRequestByMonthRangeQuery request, CancellationToken cancellationToken)
            {
                _logger.LogTrace("Handling GetLeaveRequestByMonthRangeQuery for Year: {Year} and Month: {Month}", request.year, request.month);

                try
                {
                    var response = await _repo.getLeaveRequestByMonthRage(request.year, request.month);

                    if(response == null || response.Count == 0)
                    {
                        _logger.LogInformation("No leave requests found for Year: {Year} and Month: {Month}", request.year, request.month);
                        return ApiHelperResponse.Failed("No leave requests found for the specified month and year");
                    }

                    return ApiHelperResponse.Success("success getting data", response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while handling GetLeaveRequestByMonthRangeQuery for Year: {Year} and Month: {Month}", request.month, request.month);
                    return ApiHelperResponse.Failed("Failed to delete leave request");
                }

            }



        }
    }
}
