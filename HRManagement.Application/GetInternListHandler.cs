using HRManagement.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application
{
    public record GetInternListQuery(int ProgramId, int PageNumber, string Search, string Role) : IRequest<object>;

    public class GetInternListHandler : IRequestHandler<GetInternListQuery, object>
    {
        private const int PageSize = 6;

        private readonly IELearningRepository _repo;

        public GetInternListHandler(IELearningRepository repo) => _repo = repo;

        public async Task<object> Handle(GetInternListQuery request, CancellationToken ct)
        {
            var result = await _repo.GetInternsPagedAsync(request.ProgramId, request.PageNumber, PageSize, request.Search, request.Role);

            return new
            {
                interns = result.Interns,
                totalItems = result.TotalCount,
                currentPage = request.PageNumber,
                pageSize = PageSize
            };
        }
    }
}
