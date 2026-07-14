using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using MediatR;

namespace HRManagement.Application.Commands.ELearningCommands
{
    public class DeleteModuleCommand(DeleteModuleDto dto) : IRequest<Result>
    {
        public DeleteModuleDto Dto { get; set; } = dto;
    }

    internal class DeleteModuleHandler : IRequestHandler<DeleteModuleCommand, Result>
    {
        private readonly ILogger<DeleteModuleHandler> _logger;
        private readonly IELearningRepository _repo;

        public DeleteModuleHandler(IELearningRepository repo, ILogger<DeleteModuleHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteModuleCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(DeleteModuleHandler));
            try
            {
                var success = await _repo.DeleteModuleAsync(request.Dto.moduleId, request.Dto.currentUserId.ToString());
                if (!success) return Result.Failure("Failed to delete module");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting module");
                return Result.Failure(ex.Message);
            }
        }
    }
}