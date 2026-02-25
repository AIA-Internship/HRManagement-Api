using HRManagement.Api.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application
{
    public record GetInternDashboardQuery(int UserId, string Role) : IRequest<object>;

    public class GetInternDashboardHandler : IRequestHandler<GetInternDashboardQuery, object>
    {
        private readonly IELearningRepository _repo;

        public GetInternDashboardHandler(IELearningRepository repo)
        {
            _repo = repo;
        }

        public async Task<object> Handle(GetInternDashboardQuery request, CancellationToken ct)
        {
            var completed = await _repo.GetCompletedModulesCountAsync(request.UserId);
            var total = await _repo.GetTotalModulesCountByRoleAsync(request.Role);

            return new
            {
                completedModules = completed,
                totalModules = total,
                completionPercentage = total > 0 ? (double)completed / total * 100 : 0
            };
        }
    }
}