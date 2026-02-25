using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application
{
    public record CreateModuleCommand(
        string Title,
        string Description,
        string Role,
        bool IsPriority,
        long CurrentUserId
    ) : IRequest<int>;

    public class CreateModuleHandler : IRequestHandler<CreateModuleCommand, int>
    {
        private readonly IELearningRepository _repo;

        public CreateModuleHandler(IELearningRepository repo) => _repo = repo;

        public async Task<int> Handle(CreateModuleCommand request, CancellationToken ct)
        {
            var newModule = new ModuleModel
            {
                ModuleTitle = request.Title,
                ModuleDescription = request.Description,
                TargetRole = request.Role,
                IsPriority = request.IsPriority,
                CreatedBy = request.CurrentUserId.ToString(),
                CreatedUtcDate = DateTime.UtcNow
            };

            return await _repo.CreateModuleAsync(newModule);
        }
    }
}