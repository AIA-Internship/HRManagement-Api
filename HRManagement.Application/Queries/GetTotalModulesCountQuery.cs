using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Application.Queries
{
    public class GetTotalModulesCountQuery(string role) : IRequest<Result<int>>
    {
        public string Role { get; set; } = role;
    }

    internal class GetTotalModulesCountHandler(IELearningRepository repo, ILogger<GetTotalModulesCountHandler> logger)
        : IRequestHandler<GetTotalModulesCountQuery, Result<int>>
    {
        public async Task<Result<int>> Handle(GetTotalModulesCountQuery request, CancellationToken ct)
        {
            logger.LogTrace("Executing handler for request : {request}", nameof(GetTotalModulesCountHandler));
            var count = await repo.GetTotalModulesCountByRoleAsync(request.Role);
            return Result.Success(count);
        }
    }

   
}
