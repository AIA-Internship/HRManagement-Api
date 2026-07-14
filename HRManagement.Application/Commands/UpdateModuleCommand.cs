using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using MediatR;

namespace HRManagement.Application.Commands.ELearningCommands
{
    public class UpdateModuleCommand(UpdateModuleDto dto) : IRequest<Result>
    {
        public UpdateModuleDto Dto { get; set; } = dto;
    }

    internal class UpdateModuleHandler : IRequestHandler<UpdateModuleCommand, Result>
    {
        private readonly ILogger<UpdateModuleHandler> _logger;
        private readonly IELearningRepository _repo;

        public UpdateModuleHandler(IELearningRepository repo, ILogger<UpdateModuleHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateModuleCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(UpdateModuleHandler));
            try
            {
                var entity = new ModuleModel
                {
                    ModuleId = request.Dto.moduleId,
                    BatchId = request.Dto.batchId,
                    ModuleTitle = request.Dto.title,
                    ModuleDescription = request.Dto.description,
                    TargetRole = request.Dto.role,
                    IsPriority = request.Dto.isPriority,
                    DueDate = request.Dto.dueDate,
                    ModifiedBy = request.Dto.currentUserId.ToString(),
                    ModifiedUtcDate = DateTime.UtcNow
                };

                var success = await _repo.UpdateModuleAsync(entity);
                if (!success) return Result.Failure("Module update failed");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating module");
                return Result.Failure(ex.Message);
            }
        }
    }
}