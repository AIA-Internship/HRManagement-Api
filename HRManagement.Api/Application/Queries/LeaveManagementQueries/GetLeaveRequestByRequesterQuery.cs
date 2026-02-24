using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Interfaces.NewFolder;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
using HRManagement.Api.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HRManagement.Api.Application.Queries.LeaveManagementQueries
{
    public class GetLeaveRequestByRequesterQuery(int requsterId): IRequest<Result<ApiResponse>>
    {
        public int RequesterId { get; set; } = requsterId;


    }
    internal class GetLeaveRequestByRequesterQueryHandler: IRequestHandler<GetLeaveRequestByRequesterQuery, Result<ApiResponse>>
    {
        private readonly ILogger<LoginQueryHandler> _logger;
        private readonly ILeaveRequestRepository _repo;

        public GetLeaveRequestByRequesterQueryHandler(
            ILeaveRequestRepository repo
            , ILogger<LoginQueryHandler> logger
            , IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _logger = logger;
        }
        public async Task<Result<ApiResponse>> Handle(GetLeaveRequestByRequesterQuery request, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(LoginQueryHandler));

            try
            {
                var entity = await _repo.getLeaveRequestsByRequesterId(request.RequesterId);

                if (entity == null) return ApiHelperResponse.Failed("data not found in system");


                List<ReadLeaveRequestDto> data = new List<ReadLeaveRequestDto>();

                foreach(var d in entity)
                {
                    data.Add(LeaveRequestMapping.mapToReadDto(d));
                }


                return ApiHelperResponse.Success("data retrieved successfully", data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }   
    }   
}
