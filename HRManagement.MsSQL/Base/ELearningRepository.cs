using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.Models.Tables.ELearningModels;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.MsSQL.Base
{
    public class ELearningRepository : IELearningRepository
    {
        private readonly AppDbContext _context;

        public AppDbContext Context => _context;

        public ELearningRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateModuleAsync(ModuleModel entity)
        {
            Context.ELearningModules.Add(entity);
            await Context.SaveChangesAsync();
            return entity.ModuleId;
        }

        public async Task<bool> UpdateModuleAsync(ModuleModel entity)
        {
            var existing = await Context.ELearningModules.FindAsync(entity.ModuleId);
            if (existing == null) return false;

            if (entity.BatchId > 0)
            {
                existing.BatchId = entity.BatchId;
            }
            existing.ModuleTitle = entity.ModuleTitle;
            existing.ModuleDescription = entity.ModuleDescription;
            existing.TargetRole = entity.TargetRole;
            existing.DueDate = entity.DueDate;
            existing.ModifiedUtcDate = DateTime.UtcNow;

            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteModuleAsync(int moduleId, string currentUserId)
        {
            var entity = await Context.ELearningModules.FindAsync(moduleId);
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.ModifiedBy = currentUserId;
            entity.ModifiedUtcDate = DateTime.UtcNow;

            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> GradeSubmissionAsync(int submissionId, decimal score, long graderId)
        {
            var submission = await Context.ELearningQuizSubmissions.FindAsync(submissionId);
            if (submission == null) return false;

            submission.TotalScore = score;

            submission.GradedUtcDate = DateTime.UtcNow;

            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<dynamic> Interns, int TotalCount)> GetInternsPagedAsync(int targetProgramId, int pageNumber, int pageSize, string search, string role)
        {
            var programModules = await (from batch in _context.ELearningBatches
                                        join mdle in _context.ELearningModules on batch.BatchId equals mdle.BatchId
                                        where (targetProgramId == 0 || batch.ProgramId == targetProgramId) && 
                                              !mdle.IsDeleted
                                        select mdle).ToListAsync();

            var cohortQuery = from member in _context.ELearningGroupMembers
                              join prog in _context.ELearningPrograms on member.GroupId equals prog.GroupId
                              join employee in _context.Employee on member.EmployeeId equals employee.Id
                              join u in _context.Users on employee.Id equals u.EmployeeId
                              join emp in _context.EmploymentInformation on employee.Id equals emp.EmployeeId into empJoin
                              from emp in empJoin.DefaultIfEmpty()
                              where (targetProgramId == 0 || prog.ProgramId == targetProgramId) && u.RoleId == 2
                              select new { User = u, Employee = employee, PositionName = emp.PositionName };

            if (!string.IsNullOrEmpty(search))
                cohortQuery = cohortQuery.Where(x => x.Employee.FullName.Contains(search));

            if (!string.IsNullOrEmpty(role))
                cohortQuery = cohortQuery.Where(x => x.PositionName == role);

            int totalCount = await cohortQuery.CountAsync();

            var rawInternData = await cohortQuery
                .OrderBy(x => x.Employee.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var compiledList = new List<dynamic>();

            var allModuleIdsInScope = programModules.Select(m => m.ModuleId).ToList();
            var allQuizzesInScope = await _context.ELearningQuizzes
                .Where(q => allModuleIdsInScope.Contains(q.ModuleId) && !q.IsDeleted)
                .ToListAsync();

            foreach (var row in rawInternData)
            {
                var intern = row.User;
                var internRole = (row.PositionName ?? "Intern").Trim();

                var internModules = programModules
                    .Where(m => (m.TargetRole ?? "").Trim() == "All" || (m.TargetRole ?? "").Trim() == internRole)
                    .ToList();
                var internModuleIds = internModules.Select(m => m.ModuleId).ToList();
                int totalModulesCountDenominator = internModuleIds.Count;

                var internQuizIds = allQuizzesInScope
                    .Where(q => internModuleIds.Contains(q.ModuleId))
                    .Select(q => q.QuizId)
                    .ToList();
                int totalQuizzesCountDenominator = internQuizIds.Count;

                int itemsCompletedNumerator = 0;
                if (internModuleIds.Any())
                {
                    itemsCompletedNumerator = await _context.ELearningModuleProgress
                        .CountAsync(p => p.EmployeeId == intern.Id &&
                                         internModuleIds.Contains(p.ModuleId) &&
                                         p.ProgressStatus == "Completed");
                }

                var latestQuizSubmissions = new List<HRManagement.Domain.Models.Tables.ELearningModels.QuizSubmissionModel>();
                if (internQuizIds.Any())
                {
                    var allQuizSubmissionsForIntern = await _context.ELearningQuizSubmissions
                        .Where(s => s.UserId == intern.Id && internQuizIds.Contains(s.QuizId))
                        .ToListAsync();

                    latestQuizSubmissions = allQuizSubmissionsForIntern
                        .GroupBy(s => s.QuizId)
                        .Select(g => g.OrderByDescending(x => x.CreatedUtcDate).First())
                        .ToList();
                }

                var latestScores = latestQuizSubmissions
                    .Where(s => s.TotalScore != null)
                    .Select(s => s.TotalScore.Value)
                    .ToList();

                string accumulativeScoreDisplay = latestScores.Any()
                    ? $"{Math.Round(latestScores.Average(), 0)}/100"
                    : "No submissions yet";

                int quizzesPassedNumerator = latestQuizSubmissions.Count(s => s.IsPassed == true);

                compiledList.Add(new
                {
                    EmployeeId = intern.Id,
                    Name = row.Employee.FullName,
                    Role = row.PositionName ?? "Intern",
                    TotalModulesCompletedText = $"{itemsCompletedNumerator} / {totalModulesCountDenominator}",
                    TotalQuizzesCompletedText = $"{quizzesPassedNumerator} / {totalQuizzesCountDenominator}",
                    AccumulativeScoreDisplay = accumulativeScoreDisplay
                });
            }

            return (compiledList, totalCount);
        }
        public async Task<int> GetCompletedModulesCountAsync(int userId)
        {
            var cohortModuleIds = await (from member in Context.ELearningGroupMembers
                                         join prog in Context.ELearningPrograms on member.GroupId equals prog.GroupId
                                         join employee in Context.Employee on member.EmployeeId equals employee.Id
                                         join u in Context.Users on employee.Id equals u.EmployeeId
                                         join batch in Context.ELearningBatches on prog.ProgramId equals batch.ProgramId
                                         join mdle in Context.ELearningModules on batch.BatchId equals mdle.BatchId
                                         where u.Id == userId && !mdle.IsDeleted
                                         select mdle.ModuleId)
                                         .ToListAsync();

            return await Context.ELearningModuleProgress
                .CountAsync(p => p.EmployeeId == userId &&
                                 cohortModuleIds.Contains(p.ModuleId) &&
                                 p.ProgressStatus == "Completed");
        }

        public async Task<int> GetTotalModulesCountByRoleAsync(string role)
        {
            return await Context.ELearningModules
                .CountAsync(m => (m.TargetRole == role || m.TargetRole == "All") && !m.IsDeleted);
        }

        public async Task<ModuleModel> GetModuleByIdAsync(int moduleId)
        {
            return await Context.ELearningModules
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId && !m.IsDeleted);
        }

        public async Task<int> AddContentAsync(ModuleContentModel entity)
        {
            var lastOrder = await Context.ELearningModuleContents
                .Where(c => c.ModuleId == entity.ModuleId)
                .MaxAsync(c => (int?)c.SortOrder) ?? 0;

            entity.SortOrder = lastOrder + 1;

            Context.ELearningModuleContents.Add(entity);
            await Context.SaveChangesAsync();
            return entity.ContentId;
        }

        public async Task<IEnumerable<ModuleContentModel>> GetContentsByModuleIdAsync(int moduleId)
        {
            return await Context.ELearningModuleContents
                .Where(c => c.ModuleId == moduleId && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }

        public async Task<ModuleContentModel?> GetContentByIdAsync(int contentId)
        {
            return await Context.ELearningModuleContents
                .FirstOrDefaultAsync(c => c.ContentId == contentId && !c.IsDeleted);
        }

        public async Task<bool> DeleteContentAsync(int contentId, string currentUserId)
        {
            var content = await Context.ELearningModuleContents
                .FirstOrDefaultAsync(c => c.ContentId == contentId && !c.IsDeleted);

            if (content == null) return false;

            content.IsDeleted = true;
            content.ModifiedBy = currentUserId;
            content.ModifiedUtcDate = DateTime.UtcNow;

            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<QuizModel>> GetQuizzesByModuleIdAsync(int moduleId)
        {
            return await Context.ELearningQuizzes
                .Where(q => q.ModuleId == moduleId && !q.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<ModuleModel>> GetModulesByProgramIdAsync(int programId)
        {
            return await (from batch in Context.ELearningBatches
                          join mdle in Context.ELearningModules on batch.BatchId equals mdle.BatchId
                          where batch.ProgramId == programId && !mdle.IsDeleted
                          select mdle).ToListAsync();
        }

        public async Task<IEnumerable<QuizModel>> GetQuizzesByProgramIdAsync(int programId)
        {
            return await (from batch in Context.ELearningBatches
                          join mdle in Context.ELearningModules on batch.BatchId equals mdle.BatchId
                          join quiz in Context.ELearningQuizzes on mdle.ModuleId equals quiz.ModuleId
                          where batch.ProgramId == programId && !mdle.IsDeleted && !quiz.IsDeleted
                          select quiz).ToListAsync();
        }

        public async Task<IEnumerable<QuizSubmissionModel>> GetSubmissionsByUserAndQuizIdsAsync(int userId, IEnumerable<int> quizIds)
        {
            return await Context.ELearningQuizSubmissions
                .Where(s => s.UserId == userId && quizIds.Contains(s.QuizId))
                .ToListAsync();
        }

        public async Task<int> GetQuestionCountByQuizIdAsync(int quizId)
        {
            return await Context.ELearningQuizQuestions
                .CountAsync(q => q.QuizId == quizId);
        }

        public async Task<IEnumerable<ModuleModel>> GetAvailableModulesAsync(string role, string search)
        {
            var query = Context.ELearningModules.Where(m => !m.IsDeleted);

            if (!string.IsNullOrEmpty(role))
                query = query.Where(m => m.TargetRole == role || m.TargetRole == "All");

            if (!string.IsNullOrEmpty(search))
                query = query.Where(m => m.ModuleTitle.Contains(search));

            return await query
                .OrderBy(m => m.ModuleTitle)
                .ToListAsync();
        }

        public async Task<IEnumerable<ModuleModel>> GetModulesByBatchIdAsync(int batchId, string search, IEnumerable<string> roles)
        {
            var query = Context.ELearningModules.Where(m => m.BatchId == batchId && !m.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.ModuleTitle.Contains(search));

            var roleList = roles?.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.ToLower()).ToList();
            if (roleList != null && roleList.Any())
                query = query.Where(m => roleList.Contains(m.TargetRole.ToLower()) || m.TargetRole.ToLower() == "all");

            return await query
                .OrderBy(m => m.ModuleTitle)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuizSubmissionModel>> GetSubmissionsByQuizIdAsync(int quizId)
        {
            return await Context.ELearningQuizSubmissions
                .Where(s => s.QuizId == quizId)
                .ToListAsync();
        }

        public async Task<IEnumerable<InternInfoDto>> GetEligibleInternsForQuizAsync(int quizId)
        {
            // Get the module's target role for this quiz
            var targetRole = await (from quiz in Context.ELearningQuizzes
                                    join mdle in Context.ELearningModules on quiz.ModuleId equals mdle.ModuleId
                                    where quiz.QuizId == quizId
                                    select mdle.TargetRole)
                                   .FirstOrDefaultAsync();

            // Get all employee IDs in the program's group
            var memberEmployeeIds = await (from quiz in Context.ELearningQuizzes
                                           join mdle in Context.ELearningModules on quiz.ModuleId equals mdle.ModuleId
                                           join batch in Context.ELearningBatches on mdle.BatchId equals batch.BatchId
                                           join prog in Context.ELearningPrograms on batch.ProgramId equals prog.ProgramId
                                           join member in Context.ELearningGroupMembers on prog.GroupId equals member.GroupId
                                           where quiz.QuizId == quizId
                                           select member.EmployeeId)
                                          .Distinct()
                                          .ToListAsync();

            bool isAllRole = string.IsNullOrWhiteSpace(targetRole) || targetRole.ToLower() == "all";

            if (isAllRole)
            {
                // All program members are eligible
                return await Context.Employee
                    .Where(e => memberEmployeeIds.Contains(e.Id) && e.IsActive)
                    .Select(e => new InternInfoDto { UserId = e.Id, FullName = e.FullName })
                    .ToListAsync();
            }
            else
            {
                // Only members whose position matches the module's target role
                return await (from e in Context.Employee
                              join emp in Context.EmploymentInformation on e.Id equals emp.EmployeeId into empJoin
                              from emp in empJoin.DefaultIfEmpty()
                              where memberEmployeeIds.Contains(e.Id) && e.IsActive
                                    && emp.PositionName == targetRole
                              select new InternInfoDto { UserId = e.Id, FullName = e.FullName })
                             .ToListAsync();
            }
        }

        public async Task<QuizSubmissionModel?> GetSubmissionByIdAsync(int submissionId)
        {
            return await Context.ELearningQuizSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
        }

        public async Task<QuizModel?> GetQuizByIdAsync(int quizId)
        {
            return await Context.ELearningQuizzes
                .FirstOrDefaultAsync(q => q.QuizId == quizId && !q.IsDeleted);
        }

        public async Task<IEnumerable<QuizQuestionModel>> GetQuestionsByQuizIdAsync(int quizId)
        {
            return await Context.ELearningQuizQuestions
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuizQuestionOptionModel>> GetOptionsByQuestionIdsAsync(IEnumerable<int> questionIds)
        {
            return await Context.ELearningQuizQuestionOptions
                .Where(o => questionIds.Contains(o.QuestionId))
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentAnswerModel>> GetAnswersBySubmissionIdAsync(int submissionId)
        {
            return await Context.ELearningStudentAnswers
                .Where(a => a.SubmissionId == submissionId)
                .ToListAsync();
        }

        public async Task<InternInfoDto?> GetUserByIdAsync(int userId)
        {
            return await Context.Employee
                          .Where(e => e.Id == userId && e.IsActive)
                          .Select(e => new InternInfoDto { UserId = e.Id, FullName = e.FullName })
                          .FirstOrDefaultAsync();
        }

        public async Task<bool> MarkContentAsOpenedAsync(int userId, int contentId)
        {
            var content = await Context.ELearningModuleContents
                .FirstOrDefaultAsync(c => c.ContentId == contentId && !c.IsDeleted);

            if (content == null) return false;

            var alreadyOpened = await Context.ELearningContentProgress
                .AnyAsync(cp => cp.EmployeeId == userId && cp.ContentId == contentId);

            if (!alreadyOpened)
            {
                Context.ELearningContentProgress.Add(new ContentProgressModel
                {
                    EmployeeId = userId,
                    ContentId = contentId,
                    OpenedUtcDate = DateTime.UtcNow
                });
            }

            var progress = await Context.ELearningModuleProgress
                .FirstOrDefaultAsync(p => p.EmployeeId == userId && p.ModuleId == content.ModuleId);

            if (progress == null)
            {
                progress = new ProgressModel
                {
                    EmployeeId = userId,
                    ModuleId = content.ModuleId,
                    ProgressStatus = "In Progress"
                };
                Context.ELearningModuleProgress.Add(progress);
            }
            else if (progress.ProgressStatus == "Not Started")
            {
                progress.ProgressStatus = "In Progress";
            }

            // After adding content progress, ensure context has it or just save changes first to query it back
            await Context.SaveChangesAsync();

            // Re-evaluate module completion
            var quizzes = await Context.ELearningQuizzes
                .Where(q => q.ModuleId == content.ModuleId && !q.IsDeleted)
                .ToListAsync();

            var submissions = await Context.ELearningQuizSubmissions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            bool allPassed = true;
            foreach (var q in quizzes)
            {
                var latestSub = submissions.Where(s => s.QuizId == q.QuizId).OrderByDescending(s => s.CreatedUtcDate).FirstOrDefault();
                if (latestSub == null || latestSub.TotalScore < q.MinimumPassingScore)
                {
                    allPassed = false;
                    break;
                }
            }

            var allContents = await Context.ELearningModuleContents
                .Where(c => c.ModuleId == content.ModuleId && !c.IsDeleted)
                .ToListAsync();

            var openedContents = await Context.ELearningContentProgress
                .Where(cp => cp.EmployeeId == userId)
                .Select(cp => cp.ContentId)
                .ToListAsync();

            bool allContentsOpened = allContents.All(c => openedContents.Contains(c.ContentId));

            if (allPassed && allContentsOpened)
            {
                progress.ProgressStatus = "Completed";
                await Context.SaveChangesAsync();
            }

            await Context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<int>> GetOpenedContentIdsByUserAndModuleAsync(int userId, int moduleId)
        {
            var contentIds = await Context.ELearningModuleContents
                .Where(c => c.ModuleId == moduleId && !c.IsDeleted)
                .Select(c => c.ContentId)
                .ToListAsync();

            return await Context.ELearningContentProgress
                .Where(cp => cp.EmployeeId == userId && contentIds.Contains(cp.ContentId))
                .Select(cp => cp.ContentId)
                .ToListAsync();
        }

        public async Task<bool> SubmitQuizAsync(QuizSubmissionModel entity)
        {
            var strategy = Context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await Context.Database.BeginTransactionAsync();
                try
                {
                    Context.ELearningQuizSubmissions.Add(entity);

                    var currentQuiz = await Context.ELearningQuizzes
                        .FirstOrDefaultAsync(q => q.QuizId == entity.QuizId);

                    if (currentQuiz == null) return false;

                    var progress = await Context.ELearningModuleProgress
                        .FirstOrDefaultAsync(p => p.EmployeeId == entity.UserId && p.ModuleId == currentQuiz.ModuleId);

                    if (progress == null)
                    {
                        Context.ELearningModuleProgress.Add(new ProgressModel
                        {
                            EmployeeId = entity.UserId,
                            ModuleId = currentQuiz.ModuleId,
                            ProgressStatus = "Completed"
                        });
                    }
                    else
                    {
                        progress.ProgressStatus = "Completed";
                    }

                    await Context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            });
        }


        public async Task<IEnumerable<ProgressModel>> GetProgressRecordsByEmployeeAsync(int employeeId)
        {
            return await _context.Set<ProgressModel>()
                .Where(p => p.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<int> GetTotalCohortModulesCountAsync(int employeeId)
        {
            return await (from member in _context.Set<GroupMemberModel>()
                          join prog in _context.Set<ProgramModel>() on member.GroupId equals prog.GroupId
                          join employee in _context.Employee on member.EmployeeId equals employee.Id
                          join u in _context.Users on employee.Id equals u.EmployeeId
                          join batch in _context.Set<BatchModel>() on prog.ProgramId equals batch.ProgramId
                          join mdle in _context.Set<ModuleModel>() on batch.BatchId equals mdle.BatchId
                          join emp in _context.EmploymentInformation on employee.Id equals emp.EmployeeId into empJoin
                          from emp in empJoin.DefaultIfEmpty()
                          where u.Id == employeeId && mdle.IsDeleted == false
                                && (mdle.TargetRole.ToLower() == "all" || mdle.TargetRole.ToLower() == emp.PositionName.ToLower())
                          select mdle.ModuleId)
                          .CountAsync();
        }

        public async Task<int> GetCompletedCohortModulesCountAsync(int employeeId)
        {
            return await _context.Set<ProgressModel>()
                .Where(p => p.EmployeeId == employeeId && p.ProgressStatus == "Completed")
                .CountAsync();
        }

        public async Task<IEnumerable<ModuleModel>> GetUpcomingCohortDeadlinesAsync(int employeeId, DateTime fromDateInclusive, DateTime toDateInclusive)
        {
            var cohortModules = from member in _context.Set<GroupMemberModel>()
                                 join prog in _context.Set<ProgramModel>() on member.GroupId equals prog.GroupId
                                 join employee in _context.Employee on member.EmployeeId equals employee.Id
                                 join u in _context.Users on employee.Id equals u.EmployeeId
                                 join batch in _context.Set<BatchModel>() on prog.ProgramId equals batch.ProgramId
                                 join mdle in _context.Set<ModuleModel>() on batch.BatchId equals mdle.BatchId
                                 join emp in _context.EmploymentInformation on employee.Id equals emp.EmployeeId into empJoin
                                 from emp in empJoin.DefaultIfEmpty()
                                 where u.Id == employeeId
                                       && !mdle.IsDeleted
                                       && mdle.DueDate != null
                                       && mdle.DueDate >= fromDateInclusive
                                       && mdle.DueDate <= toDateInclusive
                                       && (mdle.TargetRole.ToLower() == "all" || mdle.TargetRole.ToLower() == emp.PositionName.ToLower())
                                 select mdle;

            var completedModuleIds = await _context.Set<ProgressModel>()
                .Where(p => p.EmployeeId == employeeId && p.ProgressStatus == "Completed")
                .Select(p => p.ModuleId)
                .ToListAsync();

            return await cohortModules
                .Where(m => !completedModuleIds.Contains(m.ModuleId))
                .OrderBy(m => m.DueDate)
                .ToListAsync();
        }

        public async Task<bool> AssignInternToGroupAsync(int employeeId, DateTime createdDate)
        {
            int targetGroupYear = createdDate.Year;
            int month = createdDate.Month;

            if (month >= 9 && month <= 12)
            {
                targetGroupYear = createdDate.Year + 1;
            }
            else if (month == 1)
            {
                targetGroupYear = createdDate.Year;
            }
            else
            {
                targetGroupYear = createdDate.Year;
            }

            string targetGroupName = $"Internship {targetGroupYear}";

            var existingGroup = await Context.Set<GroupModel>()
                .FirstOrDefaultAsync(g => g.GroupName == targetGroupName);

            int groupId;

            if (existingGroup == null)
            {
                var newGroup = new GroupModel
                {
                    GroupName = targetGroupName
                };

                Context.Set<GroupModel>().Add(newGroup);
                await Context.SaveChangesAsync();

                groupId = newGroup.GroupId;
            }
            else
            {
                groupId = existingGroup.GroupId;
            }

            var newMemberLink = new GroupMemberModel
            {
                GroupId = groupId,
                EmployeeId = employeeId
            };

            Context.Set<GroupMemberModel>().Add(newMemberLink);
            await Context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> SoftDeleteModuleAsync(int moduleId, string currentUserId)
        {
            var module = await _context.Set<ModuleModel>()
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId);

            if (module == null) return false;

            module.IsDeleted = true;
            module.ModifiedBy = currentUserId;
            module.ModifiedUtcDate = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteQuizAsync(int quizId)
        {
            var quiz = await _context.Set<QuizModel>()
                .FirstOrDefaultAsync(q => q.QuizId == quizId && !q.IsDeleted);

            if (quiz == null) return false;

            quiz.IsDeleted = true;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> CreateProgramAsync(ProgramModel entity)
        {
            Context.ELearningPrograms.Add(entity);
            await Context.SaveChangesAsync();
            return entity.ProgramId;
        }

        public async Task<bool> GroupExistsAsync(int groupId)
        {
            return await Context.ELearningGroups.AnyAsync(g => g.GroupId == groupId);
        }

        public async Task<IEnumerable<GroupModel>> GetAllGroupsAsync()
        {
            return await Context.ELearningGroups.ToListAsync();
        }

        public async Task<IEnumerable<ProgramModel>> GetAllProgramsAsync()
        {
            return await Context.ELearningPrograms.ToListAsync();
        }

        public async Task<int> CreateBatchAsync(BatchModel entity)
        {
            Context.ELearningBatches.Add(entity);
            await Context.SaveChangesAsync();
            return entity.BatchId;
        }

        public async Task<bool> ProgramExistsAsync(int programId)
        {
            return await Context.ELearningPrograms.AnyAsync(p => p.ProgramId == programId);
        }

        public async Task<IEnumerable<BatchModel>> GetBatchesByProgramIdAsync(int programId)
        {
            return await Context.ELearningBatches
                .Where(b => programId == 0 || b.ProgramId == programId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BatchModel>> GetBatchesByEmployeeIdAsync(int employeeId)
        {
            return await (from member in _context.Set<GroupMemberModel>()
                          join prog in _context.Set<ProgramModel>() on member.GroupId equals prog.GroupId
                          join employee in _context.Employee on member.EmployeeId equals employee.Id
                          join u in _context.Users on employee.Id equals u.EmployeeId
                          join batch in _context.Set<BatchModel>() on prog.ProgramId equals batch.ProgramId
                          where u.Id == employeeId
                          select batch)
                          .Distinct()
                          .ToListAsync();
        }

        public async Task<BatchModel?> GetBatchByIdAsync(int batchId)
        {
            return await Context.ELearningBatches
                .FirstOrDefaultAsync(b => b.BatchId == batchId);
        }

        public async Task<IEnumerable<int>> GetOpenedContentIdsByEmployeeAsync(int employeeId)
        {
            return await Context.ELearningContentProgress
                .Where(cp => cp.EmployeeId == employeeId)
                .Select(cp => cp.ContentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ModuleModel>> GetModulesByEmployeeCohortAsync(int employeeId, string search)
        {
            var query = from member in _context.Set<GroupMemberModel>()
                        join prog in _context.Set<ProgramModel>() on member.GroupId equals prog.GroupId
                        join employee in _context.Employee on member.EmployeeId equals employee.Id
                        join u in _context.Users on employee.Id equals u.EmployeeId
                        join batch in _context.Set<BatchModel>() on prog.ProgramId equals batch.ProgramId
                        join mdle in _context.Set<ModuleModel>() on batch.BatchId equals mdle.BatchId
                        join emp in _context.EmploymentInformation on employee.Id equals emp.EmployeeId into empJoin
                        from emp in empJoin.DefaultIfEmpty()
                        where u.Id == employeeId && mdle.IsDeleted == false
                              && (mdle.TargetRole.ToLower() == "all" || mdle.TargetRole.ToLower() == emp.PositionName.ToLower())
                        orderby batch.EndDate ascending, mdle.ModuleTitle ascending
                        select mdle;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.ModuleTitle.Contains(search));
            }

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<string>> GetDistinctPositionsByProgramIdAsync(int programId)
        {
            return await (from member in Context.ELearningGroupMembers
                          join prog in Context.ELearningPrograms on member.GroupId equals prog.GroupId
                          join employee in Context.Employee on member.EmployeeId equals employee.Id
                          join u in Context.Users on employee.Id equals u.EmployeeId
                          join emp in Context.EmploymentInformation on employee.Id equals emp.EmployeeId into empJoin
                          from emp in empJoin.DefaultIfEmpty()
                          where prog.ProgramId == programId && u.RoleId == 2
                                && emp.PositionName != null && emp.PositionName != ""
                          select emp.PositionName)
                          .Distinct()
                          .OrderBy(p => p)
                          .ToListAsync();
        }

    }
}