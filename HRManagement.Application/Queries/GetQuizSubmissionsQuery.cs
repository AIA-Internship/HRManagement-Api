using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Queries
{
    public class GetQuizSubmissionsQuery(int quizId) : IRequest<Result<ReadQuizSubmissionsListDto>>
    {
        public int QuizId { get; set; } = quizId;
    }

    internal class GetQuizSubmissionsHandler : IRequestHandler<GetQuizSubmissionsQuery, Result<ReadQuizSubmissionsListDto>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetQuizSubmissionsHandler> _logger;

        public GetQuizSubmissionsHandler(IELearningRepository repo, ILogger<GetQuizSubmissionsHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ReadQuizSubmissionsListDto>> Handle(GetQuizSubmissionsQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetQuizSubmissionsHandler));
            try
            {
                var eligibleInterns = await _repo.GetEligibleInternsForQuizAsync(request.QuizId);
                var submissions = await _repo.GetSubmissionsByQuizIdAsync(request.QuizId);
                var quiz = await _repo.GetQuizByIdAsync(request.QuizId);

                var latestSubmissions = submissions
                    .GroupBy(s => s.UserId)
                    .Select(g => g.OrderByDescending(s => s.CreatedUtcDate).First())
                    .ToList();

                var submittedUserIds = new HashSet<int>(latestSubmissions.Select(s => s.UserId));

                var submitted = latestSubmissions
                    .OrderByDescending(s => s.CreatedUtcDate)
                    .Select(s =>
                    {
                        var intern = eligibleInterns.FirstOrDefault(u => u.UserId == s.UserId);
                        return new ReadSubmittedItemDto
                        {
                            submissionId = s.SubmissionId,
                            userId = s.UserId,
                            name = intern?.FullName ?? "Unknown",
                            totalScore = s.TotalScore,
                            isPassed = s.IsPassed
                        };
                    })
                    .ToList();

                var notSubmitted = eligibleInterns
                    .Where(u => !submittedUserIds.Contains(u.UserId))
                    .Select(u => new ReadNotSubmittedItemDto { userId = u.UserId, name = u.FullName })
                    .ToList();

                var result = new ReadQuizSubmissionsListDto
                {
                    moduleId = quiz?.ModuleId ?? 0,
                    totalEligible = eligibleInterns.Count(),
                    submittedCount = submittedUserIds.Count,
                    submitted = submitted,
                    notSubmitted = notSubmitted
                };

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching submissions for quiz {quizId}", request.QuizId);
                return Result.Failure<ReadQuizSubmissionsListDto>(ex.Message);
            }
        }
    }
}
