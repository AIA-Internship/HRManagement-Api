using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using MediatR;

namespace HRManagement.Api.Application.Commands.ELearningCommands
{
    public class UpdateModuleCommand(UpdateModuleDto dto) : IRequest<Result<ApiResponse>>
    {
        public UpdateModuleDto Dto { get; set; } = dto;
    }

    internal class UpdateModuleHandler : IRequestHandler<UpdateModuleCommand, Result<ApiResponse>>
    {
        private readonly ILogger<UpdateModuleHandler> _logger;
        private readonly IELearningRepository _repo;

        public UpdateModuleHandler(IELearningRepository repo, ILogger<UpdateModuleHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(UpdateModuleCommand request, CancellationToken ct)
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
                if (!success) return ApiHelperResponse.Failed("Module update failed");

                return ApiHelperResponse.Success("Module updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating module");
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}