using HRManagement.Api.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application
{
    public record GetInternListQuery(int PageNumber, string Search) : IRequest<object>;

    public class GetInternListHandler : IRequestHandler<GetInternListQuery, object>
    {
        private readonly IELearningRepository _repo;

        public GetInternListHandler(IELearningRepository repo) => _repo = repo;

        public async Task<object> Handle(GetInternListQuery request, CancellationToken ct)
        {
            var result = await _repo.GetInternsPagedAsync(request.PageNumber, 6, request.Search);

            return new
            {
                interns = result.Interns,
                totalItems = result.TotalCount,
                currentPage = request.PageNumber,
                pageSize = 6
            };
        }
    }
}