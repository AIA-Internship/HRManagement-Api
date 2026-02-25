using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Queries.LeaveManagementQueries
{
    public class GetLeaveRequestByMonthRangeQuery : IRequest<Result<ApiResponse>>
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public GetLeaveRequestByMonthRangeQuery(int month, int year)
        {
            Month = month;
            Year = year;
        }
    }
}
