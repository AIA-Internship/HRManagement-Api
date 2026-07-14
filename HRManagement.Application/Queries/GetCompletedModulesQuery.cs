using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Application.Queries
{
    public class GetCompletedModulesCountQuery(int userId) : IRequest<Result<int>>
    {
        public int UserId { get; set; } = userId;
    }

    internal class GetCompletedModulesCountHandler(IELearningRepository repo, ILogger<GetCompletedModulesCountHandler> logger)
        : IRequestHandler<GetCompletedModulesCountQuery, Result<int>>
    {
        public async Task<Result<int>> Handle(GetCompletedModulesCountQuery request, CancellationToken ct)
        {
            logger.LogTrace("Executing handler for request : {request}", nameof(GetCompletedModulesCountHandler));
            var count = await repo.GetCompletedModulesCountAsync(request.UserId);
            return Result.Success(count);
        }
    }
}
