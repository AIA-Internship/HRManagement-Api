using HRManagement.Api.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application
{
    public record GradeSubmissionCommand(int SubmissionId, decimal Score, long GraderId) : IRequest<bool>;

    public class GradeSubmissionHandler : IRequestHandler<GradeSubmissionCommand, bool>
    {
        private readonly IELearningRepository _repo;
        public GradeSubmissionHandler(IELearningRepository repo) => _repo = repo;

        public async Task<bool> Handle(GradeSubmissionCommand request, CancellationToken ct)
        {
            return await _repo.GradeSubmissionAsync(request.SubmissionId, request.Score, request.GraderId);
        }
    }
}