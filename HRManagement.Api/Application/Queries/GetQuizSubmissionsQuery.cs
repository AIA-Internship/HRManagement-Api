using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Queries
{
    public class GetQuizSubmissionsQuery(int quizId) : IRequest<Result<ApiResponse>>
    {
        public int QuizId { get; set; } = quizId;
    }

    internal class GetQuizSubmissionsHandler : IRequestHandler<GetQuizSubmissionsQuery, Result<ApiResponse>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetQuizSubmissionsHandler> _logger;

        public GetQuizSubmissionsHandler(IELearningRepository repo, ILogger<GetQuizSubmissionsHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(GetQuizSubmissionsQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetQuizSubmissionsHandler));
            try
            {
                var eligibleInterns = await _repo.GetEligibleInternsForQuizAsync(request.QuizId);
                var submissions = await _repo.GetSubmissionsByQuizIdAsync(request.QuizId);

                var submittedUserIds = new HashSet<int>(submissions.Select(s => s.UserId));

                var submitted = submissions
                    .OrderByDescending(s => s.CreatedUtcDate)
                    .Select(s =>
                    {
                        var intern = eligibleInterns.FirstOrDefault(u => u.UserId == s.UserId);
                        return new
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
                    .Select(u => new { userId = u.UserId, name = u.FullName })
                    .ToList();

                var result = new
                {
                    totalEligible = eligibleInterns.Count(),
                    submittedCount = submittedUserIds.Count,
                    submitted,
                    notSubmitted
                };

                return ApiHelperResponse.Success("Quiz submissions retrieved", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching submissions for quiz {quizId}", request.QuizId);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
