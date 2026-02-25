using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application
{
    public record SubmitQuizCommand(
        int ContentId,
        int UserId,
        string OriginalFileName,
        string StoredFileName,
        string FilePath
    ) : IRequest<bool>;

    public class SubmitQuizHandler : IRequestHandler<SubmitQuizCommand, bool>
    {
        private readonly IELearningRepository _repo;
        public SubmitQuizHandler(IELearningRepository repo) => _repo = repo;

        public async Task<bool> Handle(SubmitQuizCommand request, CancellationToken ct)
        {
            var submission = new QuizSubmissionModel
            {
                ContentId = request.ContentId,
                UserId = request.UserId,
                AnswerOriginalFileName = request.OriginalFileName,
                AnswerStoredFileName = request.StoredFileName,
                AnswerFilePath = request.FilePath,
                SubmittedUtcDate = DateTime.UtcNow,
                CreatedUtcDate = DateTime.UtcNow
            };

            return await _repo.SubmitQuizAsync(submission);
        }
    }
}