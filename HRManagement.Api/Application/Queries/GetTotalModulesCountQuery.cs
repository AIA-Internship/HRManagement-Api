using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Queries
{
    public class GetTotalModulesCountQuery(string role) : IRequest<Result<ApiResponse>>
    {
        public string Role { get; set; } = role;
    }

    internal class GetTotalModulesCountHandler(IELearningRepository repo, ILogger<GetTotalModulesCountHandler> logger)
        : IRequestHandler<GetTotalModulesCountQuery, Result<ApiResponse>>
    {
        public async Task<Result<ApiResponse>> Handle(GetTotalModulesCountQuery request, CancellationToken ct)
        {
            logger.LogTrace("Executing handler for request : {request}", nameof(GetTotalModulesCountHandler));
            var count = await repo.GetTotalModulesCountByRoleAsync(request.Role);
            return ApiHelperResponse.Success("Total modules count retrieved", count);
        }
    }

   
}
