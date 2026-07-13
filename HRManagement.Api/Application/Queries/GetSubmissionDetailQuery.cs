using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Queries
{
    public class GetSubmissionDetailQuery(int submissionId) : IRequest<Result<ApiResponse>>
    {
        public int SubmissionId { get; set; } = submissionId;
    }

    internal class GetSubmissionDetailHandler : IRequestHandler<GetSubmissionDetailQuery, Result<ApiResponse>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetSubmissionDetailHandler> _logger;

        public GetSubmissionDetailHandler(IELearningRepository repo, ILogger<GetSubmissionDetailHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(GetSubmissionDetailQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetSubmissionDetailHandler));
            try
            {
                var submission = await _repo.GetSubmissionByIdAsync(request.SubmissionId);
                if (submission == null) return ApiHelperResponse.Failed("Submission not found");

                var quiz = await _repo.GetQuizByIdAsync(submission.QuizId);
                if (quiz == null) return ApiHelperResponse.Failed("Quiz configuration not found");

                var intern = await _repo.GetUserByIdAsync(submission.UserId);
                var questions = (await _repo.GetQuestionsByQuizIdAsync(submission.QuizId)).ToList();
                var answers = (await _repo.GetAnswersBySubmissionIdAsync(request.SubmissionId)).ToList();
                var options = (await _repo.GetOptionsByQuestionIdsAsync(questions.Select(q => q.QuestionId))).ToList();

                decimal mcMaxScorePerQuestion = quiz.McCount > 0 ? (decimal)100 / quiz.McCount : 0;
                const decimal essayMaxScorePerQuestion = 50;

                var answerDtos = questions
                    .OrderBy(q => q.SortOrder)
                    .Select(q =>
                    {
                        var answer = answers.FirstOrDefault(a => a.QuestionId == q.QuestionId);
                        bool isMc = q.QuestionType == "MC";

                        return new
                        {
                            answerId = answer?.AnswerId,
                            questionId = q.QuestionId,
                            questionText = q.QuestionText,
                            questionType = q.QuestionType,
                            sortOrder = q.SortOrder,
                            assignedScore = answer?.AssignedScore ?? 0,
                            maxScore = isMc ? mcMaxScorePerQuestion : essayMaxScorePerQuestion,
                            selectedOption = isMc ? answer?.SelectedOption : null,
                            essayAnswerText = isMc ? null : answer?.EssayAnswerText,
                            options = isMc
                                ? options
                                    .Where(o => o.QuestionId == q.QuestionId)
                                    .Select(o => new { optionLetter = o.OptionLetter, optionText = o.OptionText, isCorrect = o.IsCorrect })
                                    .ToList()
                                : null
                        };
                    })
                    .ToList();

                var dto = new
                {
                    submissionId = submission.SubmissionId,
                    quizId = submission.QuizId,
                    userId = submission.UserId,
                    internName = intern?.FullName ?? "Unknown",
                    totalScore = submission.TotalScore,
                    isPassed = submission.IsPassed,
                    gradedBy = submission.GradedBy,
                    gradedUtcDate = submission.GradedUtcDate,
                    answers = answerDtos
                };

                return ApiHelperResponse.Success("Submission detail retrieved", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching submission detail for submission {submissionId}", request.SubmissionId);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}
