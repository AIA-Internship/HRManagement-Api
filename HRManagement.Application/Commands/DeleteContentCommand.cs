using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using MediatR;

namespace HRManagement.Application.Commands.ELearningCommands
{
    public class DeleteContentCommand(DeleteContentDto dto) : IRequest<Result>
    {
        public DeleteContentDto Dto { get; set; } = dto;
    }

    internal class DeleteContentHandler : IRequestHandler<DeleteContentCommand, Result>
    {
        private readonly ILogger<DeleteContentHandler> _logger;
        private readonly IELearningRepository _repo;

        public DeleteContentHandler(IELearningRepository repo, ILogger<DeleteContentHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteContentCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(DeleteContentHandler));
            try
            {
                var success = await _repo.DeleteContentAsync(request.Dto.contentId, request.Dto.currentUserId.ToString());
                if (!success) return Result.Failure("Failed to delete content");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting content");
                return Result.Failure(ex.Message);
            }
        }
    }
}
