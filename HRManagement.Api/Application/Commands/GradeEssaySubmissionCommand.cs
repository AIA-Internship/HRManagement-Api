using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Repositories.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Commands.ELearningCommands
{
    public class GradeEssayAnswerDto
    {
        public int answerId { get; set; }
        public decimal score { get; set; }
    }

    public class SubmitQuizReviewCommand : IRequest<Result<ApiResponse>>
    {
        public int SubmissionId { get; set; }
        public int SupervisorId { get; set; }
        public List<GradeEssayAnswerDto> GradedEssays { get; set; } = new();
    }

    internal class SubmitQuizReviewHandler : IRequestHandler<SubmitQuizReviewCommand, Result<ApiResponse>>
    {
        private readonly SqlDbContext _context;

        public SubmitQuizReviewHandler(SqlDbContext context) => _context = context;

        public async Task<Result<ApiResponse>> Handle(SubmitQuizReviewCommand request, CancellationToken ct)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var submission = await _context.ELearningQuizSubmissions
                    .FirstOrDefaultAsync(s => s.SubmissionId == request.SubmissionId, ct);
                if (submission == null) return ApiHelperResponse.Failed("Submission history trace not found.");

                var quizConfig = await _context.ELearningQuizzes
                    .FirstOrDefaultAsync(q => q.QuizId == submission.QuizId, ct);

                // 1. Process Essay updates
                var storedAnswers = await _context.ELearningStudentAnswers
                    .Where(a => a.SubmissionId == request.SubmissionId).ToListAsync(ct);

                decimal totalEssayPointsAccumulated = 0;
                int totalEssayQuestionsCount = quizConfig.EssayCount;

                foreach (var inputItem in request.GradedEssays)
                {
                    var databaseAnswerRow = storedAnswers.FirstOrDefault(a => a.AnswerId == inputItem.answerId);
                    if (databaseAnswerRow == null) continue;

                    databaseAnswerRow.AssignedScore = inputItem.score;
                    databaseAnswerRow.IsEvaluated = true;
                }
                await _context.SaveChangesAsync(ct);

                decimal mcComponentFinalScore = 0;
                if (quizConfig.McCount > 0)
                {
                    var mcQuestions = await _context.ELearningQuizQuestions
                        .Where(q => q.QuizId == submission.QuizId && q.QuestionType == "MC").Select(q => q.QuestionId).ToListAsync(ct);

                    decimal pointsEarned = storedAnswers.Where(a => mcQuestions.Contains(a.QuestionId)).Sum(a => a.AssignedScore);
                    mcComponentFinalScore = pointsEarned * ((decimal)quizConfig.McWeight / 100);
                }

                decimal essayComponentFinalScore = 0;
                if (quizConfig.EssayCount > 0)
                {
                    var essayQuestions = await _context.ELearningQuizQuestions
                        .Where(q => q.QuizId == submission.QuizId && q.QuestionType == "Essay").Select(q => q.QuestionId).ToListAsync(ct);

                    decimal totalRawAwarded = storedAnswers.Where(a => essayQuestions.Contains(a.QuestionId)).Sum(a => a.AssignedScore);
                    decimal maxPotentialEssayPoints = essayQuestions.Count * 50;

                    decimal essayPercentage = maxPotentialEssayPoints > 0 ? (totalRawAwarded / maxPotentialEssayPoints) * 100 : 0;
                    essayComponentFinalScore = essayPercentage * ((decimal)quizConfig.EssayWeight / 100);
                }

                submission.TotalScore = mcComponentFinalScore + essayComponentFinalScore;
                submission.IsPassed = submission.TotalScore >= quizConfig.MinimumPassingScore;
                submission.GradedBy = request.SupervisorId.ToString();
                submission.GradedUtcDate = DateTime.UtcNow;

                var currentProgress = await _context.ELearningModuleProgress
                    .FirstOrDefaultAsync(p => p.EmployeeId == submission.UserId && p.ModuleId == quizConfig.ModuleId, ct);

                string determinationStatus = submission.IsPassed == true ? "Completed" : "Failed";
                if (currentProgress != null)
                {
                    currentProgress.ProgressStatus = determinationStatus;
                    currentProgress.ModifiedUtcDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return ApiHelperResponse.Success("Student final score calculated and pushed successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}