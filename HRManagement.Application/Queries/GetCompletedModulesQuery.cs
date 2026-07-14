using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Queries
{
    public class GetCompletedModulesCountQuery(int userId) : IRequest<Result<ApiResponse>>
    {
        public int UserId { get; set; } = userId;
    }

    internal class GetCompletedModulesCountHandler(IELearningRepository repo, ILogger<GetCompletedModulesCountHandler> logger)
        : IRequestHandler<GetCompletedModulesCountQuery, Result<ApiResponse>>
    {
        public async Task<Result<ApiResponse>> Handle(GetCompletedModulesCountQuery request, CancellationToken ct)
        {
            logger.LogTrace("Executing handler for request : {request}", nameof(GetCompletedModulesCountHandler));
            var count = await repo.GetCompletedModulesCountAsync(request.UserId);
            return ApiHelperResponse.Success("Completed modules count retrieved", count);
        }
    }
}
