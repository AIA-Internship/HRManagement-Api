using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using MediatR;

namespace HRManagement.Application.Commands.ELearningCommands
{
    public class DeleteQuizCommand(DeleteQuizDto dto) : IRequest<Result>
    {
        public DeleteQuizDto Dto { get; set; } = dto;
    }

    internal class DeleteQuizHandler : IRequestHandler<DeleteQuizCommand, Result>
    {
        private readonly ILogger<DeleteQuizHandler> _logger;
        private readonly IELearningRepository _repo;

        public DeleteQuizHandler(IELearningRepository repo, ILogger<DeleteQuizHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteQuizCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(DeleteQuizHandler));
            try
            {
                var success = await _repo.DeleteQuizAsync(request.Dto.quizId);
                if (!success) return Result.Failure("Failed to delete quiz");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quiz");
                return Result.Failure(ex.Message);
            }
        }
    }
}
