using CSharpFunctionalExtensions;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Queries.LeaveManagementQueries
{
    public class GetLeaveConfig : IRequest<Result<ApiResponse>>
    {
        public GetLeaveConfig()
        {
        }
    }
    internal class GetLeaveConfigHandler : IRequestHandler<GetLeaveConfig, Result<ApiResponse>>
    {
        private readonly ILogger<GetLeaveConfig> _logger;
        private readonly ILeaveRepository _repo;
        public GetLeaveConfigHandler(
            ILeaveRepository repo
            , ILogger<GetLeaveConfig> logger
            )
        {
            _repo = repo;
            _logger = logger;
        }
        public async Task<Result<ApiResponse>> Handle(GetLeaveConfig request, CancellationToken cancellationToken)
        {
            // DEV ONLY protection
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env != "Development")
            {
                return ApiHelperResponse.Failed("[UNAUTHORIZED] This endpoint is only available in Development environment");
            }
            try
            {
                var result = await _repo.getLeaveTableConfig();
                if (result == null) return ApiHelperResponse.NotFound("Leave config not found");


                return ApiHelperResponse.Success("read leave request successfully", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ApiHelperResponse.Failed("Failed to get leave request");
            }
        }

    }
}
