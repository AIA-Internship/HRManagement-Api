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

namespace HRManagement.Application.Queries.ELearningQueries
{
    public class GetInternModuleDetailsQuery(int employeeId) : IRequest<Result<InternModuleDetailsResponseDto>>
    {
        public int EmployeeId { get; set; } = employeeId;
    }

    internal class GetInternModuleDetailsHandler : IRequestHandler<GetInternModuleDetailsQuery, Result<InternModuleDetailsResponseDto>>
    {
        private readonly IELearningRepository _repo;
        private readonly ILogger<GetInternModuleDetailsHandler> _logger;

        public GetInternModuleDetailsHandler(IELearningRepository repo, ILogger<GetInternModuleDetailsHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result<InternModuleDetailsResponseDto>> Handle(GetInternModuleDetailsQuery request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(GetInternModuleDetailsHandler));
            try
            {
                var user = await _repo.GetUserByIdAsync(request.EmployeeId);
                string internName = user?.FullName ?? "Unknown Intern";

                var modules = await _repo.GetModulesByEmployeeCohortAsync(request.EmployeeId, "");
                var progressRecords = await _repo.GetProgressRecordsByEmployeeAsync(request.EmployeeId);
                
                // Fetch quizzes for the modules to get submissions
                var allQuizzes = new List<HRManagement.Domain.Models.Tables.ELearningModels.QuizModel>();
                var allBatches = new Dictionary<int, string>();

                foreach (var batchGroup in modules.GroupBy(m => m.BatchId))
                {
                    var batch = await _repo.GetBatchByIdAsync(batchGroup.Key);
                    if (batch != null)
                    {
                        allBatches[batch.BatchId] = batch.BatchName;
                    }
                    
                    // Note: We don't have a bulk fetch for quizzes by multiple module IDs, so we iterate
                    foreach (var m in batchGroup)
                    {
                        var quizzes = await _repo.GetQuizzesByModuleIdAsync(m.ModuleId);
                        allQuizzes.AddRange(quizzes);
                    }
                }

                var quizIds = allQuizzes.Select(q => q.QuizId).ToList();
                var submissions = await _repo.GetSubmissionsByUserAndQuizIdsAsync(request.EmployeeId, quizIds);

                var result = new List<InternModuleDetailDto>();

                foreach (var m in modules)
                {
                    var progress = progressRecords.FirstOrDefault(p => p.ModuleId == m.ModuleId);
                    string currentStatus = progress?.ProgressStatus ?? "Not Started";
                    string batchName = allBatches.TryGetValue(m.BatchId, out var bName) ? bName : $"Batch {m.BatchId}";

                    if (currentStatus != "Completed")
                    {
                        var batch = await _repo.GetBatchByIdAsync(m.BatchId);
                        if (batch != null && DateTime.UtcNow.Date > batch.EndDate.Date)
                        {
                            currentStatus = "Failed";
                        }
                    }

                    // Get score if there is a quiz
                    decimal? score = null;
                    var quiz = allQuizzes.FirstOrDefault(q => q.ModuleId == m.ModuleId);
                    if (quiz != null)
                    {
                        var sub = submissions.FirstOrDefault(s => s.QuizId == quiz.QuizId);
                        if (sub != null && sub.TotalScore.HasValue)
                        {
                            score = sub.TotalScore;
                        }
                    }

                    result.Add(new InternModuleDetailDto
                    {
                        ModuleId = m.ModuleId,
                        Title = m.ModuleTitle,
                        BatchId = m.BatchId,
                        BatchName = batchName,
                        DueDate = m.DueDate,
                        ProgressStatus = currentStatus,
                        Score = score
                    });
                }

                result = result.OrderBy(m => m.DueDate).ThenBy(m => m.Title).ToList();
                
                var response = new InternModuleDetailsResponseDto
                {
                    InternName = internName,
                    Modules = result
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching module details for intern {employeeId}", request.EmployeeId);
                return Result.Failure<InternModuleDetailsResponseDto>(ex.Message);
            }
        }
    }
}
