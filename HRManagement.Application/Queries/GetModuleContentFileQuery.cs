using HRManagement.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Queries
{
    public class GetModuleContentFileQuery(int contentId) : IRequest<ModuleContentFileResult>
    {
        public int ContentId { get; set; } = contentId;
    }

    public class ModuleContentFileResult
    {
        public bool Found { get; set; }
        public string FileUrl { get; set; } = null!;
        public string DownloadFileName { get; set; } = null!;
    }

    internal class GetModuleContentFileHandler : IRequestHandler<GetModuleContentFileQuery, ModuleContentFileResult>
    {
        private readonly IELearningRepository _repo;

        public GetModuleContentFileHandler(IELearningRepository repo) => _repo = repo;

        public async Task<ModuleContentFileResult> Handle(GetModuleContentFileQuery request, CancellationToken ct)
        {
            var content = await _repo.GetContentByIdAsync(request.ContentId);
            if (content == null) return new ModuleContentFileResult { Found = false };

            return new ModuleContentFileResult
            {
                Found = true,
                FileUrl = content.ContentUrl,
                DownloadFileName = $"{content.ContentTitle}.{content.ContentType}"
            };
        }
    }
}
