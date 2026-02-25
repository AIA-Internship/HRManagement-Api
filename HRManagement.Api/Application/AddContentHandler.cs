using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application
{
    public record AddContentCommand(
        int ModuleId,
        string Title,
        bool IsQuiz,
        string FileName,
        string FilePath,
        string Extension,
        long CurrentUserId
    ) : IRequest<int>;

    public class AddContentHandler : IRequestHandler<AddContentCommand, int>
    {
        private readonly IELearningRepository _repo;
        public AddContentHandler(IELearningRepository repo) => _repo = repo;

        public async Task<int> Handle(AddContentCommand request, CancellationToken ct)
        {
            var content = new ModuleContentModel
            {
                ModuleId = request.ModuleId,
                ContentTitle = request.Title,
                IsQuiz = request.IsQuiz,
                OriginalFileName = request.FileName,
                StoredFileName = Guid.NewGuid().ToString() + request.Extension, // Clean Code: Unique filename
                FilePath = request.FilePath,
                FileExt = request.Extension,
                CreatedBy = request.CurrentUserId.ToString(),
                CreatedUtcDate = DateTime.UtcNow
            };

            return await _repo.AddContentAsync(content);
        }
    }
}