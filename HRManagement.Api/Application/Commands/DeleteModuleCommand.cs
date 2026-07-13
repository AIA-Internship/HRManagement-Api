using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using MediatR;

namespace HRManagement.Api.Application.Commands.ELearningCommands
{
    public class DeleteModuleCommand(DeleteModuleDto dto) : IRequest<Result<ApiResponse>>
    {
        public DeleteModuleDto Dto { get; set; } = dto;
    }

    internal class DeleteModuleHandler : IRequestHandler<DeleteModuleCommand, Result<ApiResponse>>
    {
        private readonly ILogger<DeleteModuleHandler> _logger;
        private readonly IELearningRepository _repo;

        public DeleteModuleHandler(IELearningRepository repo, ILogger<DeleteModuleHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(DeleteModuleCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(DeleteModuleHandler));
            try
            {
                var success = await _repo.DeleteModuleAsync(request.Dto.moduleId, request.Dto.currentUserId.ToString());
                if (!success) return ApiHelperResponse.Failed("Failed to delete module");

                return ApiHelperResponse.Success("Module soft deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting module");
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}