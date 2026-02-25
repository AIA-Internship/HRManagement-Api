using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application
{
    public record UpdateModuleCommand(
        int ModuleId,
        string Title,
        string Description,
        string Role,
        bool IsPriority,
        long ModifiedBy
    ) : IRequest<bool>;

    public class UpdateModuleHandler : IRequestHandler<UpdateModuleCommand, bool>
    {
        private readonly IELearningRepository _repo;
        public UpdateModuleHandler(IELearningRepository repo) => _repo = repo;

        public async Task<bool> Handle(UpdateModuleCommand request, CancellationToken ct)
        {
            var entity = new ModuleModel
            {
                ModuleId = request.ModuleId,
                ModuleTitle = request.Title,
                ModuleDescription = request.Description,
                TargetRole = request.Role,
                IsPriority = request.IsPriority,
                ModifiedBy = request.ModifiedBy.ToString(),
                ModifiedUtcDate = DateTime.UtcNow
            };

            return await _repo.UpdateModuleAsync(entity);
        }
    }
}