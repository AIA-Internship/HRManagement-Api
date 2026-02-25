using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application
{
    public record MarkMaterialOpenedCommand(int UserId, int ContentId) : IRequest<bool>;

    public class MarkMaterialOpenedHandler : IRequestHandler<MarkMaterialOpenedCommand, bool>
    {
        private readonly IELearningRepository _repo;
        public MarkMaterialOpenedHandler(IELearningRepository repo) => _repo = repo;

        public async Task<bool> Handle(MarkMaterialOpenedCommand request, CancellationToken ct)
        {
            return await _repo.MarkContentAsOpenedAsync(request.UserId, request.ContentId);
        }
    }
}