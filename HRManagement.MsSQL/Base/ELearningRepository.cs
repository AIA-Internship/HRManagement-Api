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

            existing.BatchId = entity.BatchId;
            existing.ModuleTitle = entity.ModuleTitle;
            existing.ModuleDescription = entity.ModuleDescription;
            existing.TargetRole = entity.TargetRole;
            existing.IsPriority = entity.IsPriority;
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

            submission.GradedBy = graderId.ToString();
            submission.GradedUtcDate = DateTime.UtcNow;
            submission.ModifiedUtcDate = DateTime.UtcNow;

            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<dynamic> Interns, int TotalCount)> GetInternsPagedAsync(int targetProgramId, int pageNumber, int pageSize, string search, string role)
        {
            var programModules = await (from batch in _context.ELearningBatches
                                        join mdle in _context.ELearningModules on batch.BatchId equals mdle.BatchId
                                        where batch.ProgramId == targetProgramId && !mdle.IsDeleted
                                        select mdle).ToListAsync();

            var programModuleIds = programModules.Select(m => m.ModuleId).ToList();
            int totalModulesCountDenominator = programModuleIds.Count;

            var programQuizIds = await _context.ELearningQuizzes
                .Where(q => programModuleIds.Contains(q.ModuleId) && !q.IsDeleted)
                .Select(q => q.QuizId)
                .ToListAsync();
            int totalQuizzesCountDenominator = programQuizIds.Count;

            var cohortQuery = from member in _context.ELearningGroupMembers
                              join prog in _context.ELearningPrograms on member.GroupId equals prog.GroupId
                              join u in _context.Users on member.EmployeeId equals u.Id
                              join employee in _context.Employee on u.EmployeeId equals employee.Id
                              join emp in _context.EmploymentInformation on u.Id equals emp.EmployeeId into empJoin
                              from emp in empJoin.DefaultIfEmpty()
                              where prog.ProgramId == targetProgramId && u.RoleId == 1
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

            foreach (var row in rawInternData)
            {
                var intern = row.User;

                int itemsCompletedNumerator = await _context.ELearningModuleProgress
                    .CountAsync(p => p.EmployeeId == intern.Id &&
                                     programModuleIds.Contains(p.ModuleId) &&
                                     p.ProgressStatus == "Completed");

                var studentSubmissions = await (from q in _context.ELearningQuizzes
                                                join s in _context.ELearningQuizSubmissions on q.QuizId equals s.QuizId
                                                where programModuleIds.Contains(q.ModuleId) && s.UserId == intern.Id && s.TotalScore != null
                                                select s.TotalScore.Value).ToListAsync();

                string accumulativeScoreDisplay = studentSubmissions.Any()
                    ? $"{Math.Round(studentSubmissions.Average(), 0)}/100"
                    : "No submissions yet";

                int quizzesPassedNumerator = await _context.ELearningQuizSubmissions
                    .Where(s => s.UserId == intern.Id && programQuizIds.Contains(s.QuizId) && s.IsPassed == true)
                    .Select(s => s.QuizId)
                    .Distinct()
                    .CountAsync();

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
                                         join batch in Context.ELearningBatches on prog.ProgramId equals batch.ProgramId
                                         join mdle in Context.ELearningModules on batch.BatchId equals mdle.BatchId
                                         where member.EmployeeId == userId && !mdle.IsDeleted
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

        public async Task<QuizModel?> GetQuizByModuleIdAsync(int moduleId)
        {
            return await Context.ELearningQuizzes
                .FirstOrDefaultAsync(q => q.ModuleId == moduleId && !q.IsDeleted);
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
                .Where(s => s.UserId == userId && quizIds.Contains(s.QuizId) && !s.IsDeleted)
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
                .OrderByDescending(m => m.IsPriority)
                .ThenBy(m => m.ModuleTitle)
                .ToListAsync();
        }

        public async Task<IEnumerable<ModuleModel>> GetModulesByBatchIdAsync(int batchId, string search, IEnumerable<string> roles)
        {
            var query = Context.ELearningModules.Where(m => m.BatchId == batchId && !m.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.ModuleTitle.Contains(search));

            var roleList = roles?.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
            if (roleList != null && roleList.Any())
                query = query.Where(m => roleList.Contains(m.TargetRole));

            return await query
                .OrderByDescending(m => m.IsPriority)
                .ThenBy(m => m.ModuleTitle)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuizSubmissionModel>> GetSubmissionsByQuizIdAsync(int quizId)
        {
            return await Context.ELearningQuizSubmissions
                .Where(s => s.QuizId == quizId && !s.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<InternInfoDto>> GetEligibleInternsForQuizAsync(int quizId)
        {
            var employeeIds = await (from quiz in Context.ELearningQuizzes
                                      join mdle in Context.ELearningModules on quiz.ModuleId equals mdle.ModuleId
                                      join batch in Context.ELearningBatches on mdle.BatchId equals batch.BatchId
                                      join prog in Context.ELearningPrograms on batch.ProgramId equals prog.ProgramId
                                      join member in Context.ELearningGroupMembers on prog.GroupId equals member.GroupId
                                      where quiz.QuizId == quizId
                                      select member.EmployeeId)
                                      .Distinct()
                                      .ToListAsync();

            return await (from u in Context.Users
                          join employee in Context.Employee on u.EmployeeId equals employee.Id
                          where employeeIds.Contains(u.Id) && !u.IsDeleted
                          select new InternInfoDto { UserId = u.Id, FullName = employee.FullName })
                          .ToListAsync();
        }

        public async Task<QuizSubmissionModel?> GetSubmissionByIdAsync(int submissionId)
        {
            return await Context.ELearningQuizSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId && !s.IsDeleted);
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
            return await (from u in Context.Users
                          join employee in Context.Employee on u.EmployeeId equals employee.Id
                          where u.Id == userId && !u.IsDeleted
                          select new InternInfoDto { UserId = u.Id, FullName = employee.FullName })
                          .FirstOrDefaultAsync();
        }

        public async Task<bool> MarkContentAsOpenedAsync(int userId, int contentId)
        {
            var content = await Context.ELearningModuleContents
                .FirstOrDefaultAsync(c => c.ContentId == contentId && !c.IsDeleted);

            if (content == null) return false;

            var progress = await Context.ELearningModuleProgress
                .FirstOrDefaultAsync(p => p.EmployeeId == userId && p.ModuleId == content.ModuleId);

            if (progress == null)
            {
                Context.ELearningModuleProgress.Add(new ProgressModel
                {
                    EmployeeId = userId,
                    ModuleId = content.ModuleId,
                    ProgressStatus = "In Progress",
                    CreatedUtcDate = DateTime.UtcNow,
                    CreatedBy = userId.ToString()
                });
            }
            else if (progress.ProgressStatus == "Not Started")
            {
                progress.ProgressStatus = "In Progress";
                progress.ModifiedUtcDate = DateTime.UtcNow;
                progress.ModifiedBy = userId.ToString();
            }

            await Context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> SubmitQuizAsync(QuizSubmissionModel entity)
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
                        ProgressStatus = "Completed",
                        CreatedUtcDate = DateTime.UtcNow,
                        CreatedBy = entity.UserId.ToString()
                    });
                }
                else
                {
                    progress.ProgressStatus = "Completed";
                    progress.ModifiedUtcDate = DateTime.UtcNow;
                    progress.ModifiedBy = entity.UserId.ToString();
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
                          join batch in _context.Set<BatchModel>() on prog.ProgramId equals batch.ProgramId
                          join mdle in _context.Set<ModuleModel>() on batch.BatchId equals mdle.BatchId
                          where member.EmployeeId == employeeId && mdle.IsDeleted == false
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
                                 join batch in _context.Set<BatchModel>() on prog.ProgramId equals batch.ProgramId
                                 join mdle in _context.Set<ModuleModel>() on batch.BatchId equals mdle.BatchId
                                 where member.EmployeeId == employeeId
                                       && !mdle.IsDeleted
                                       && mdle.DueDate != null
                                       && mdle.DueDate >= fromDateInclusive
                                       && mdle.DueDate <= toDateInclusive
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
                .Where(b => b.ProgramId == programId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ModuleModel>> GetModulesByEmployeeCohortAsync(int employeeId, string search)
        {
            var query = from member in _context.Set<GroupMemberModel>()
                        join prog in _context.Set<ProgramModel>() on member.GroupId equals prog.GroupId
                        join batch in _context.Set<BatchModel>() on prog.ProgramId equals batch.ProgramId
                        join mdle in _context.Set<ModuleModel>() on batch.BatchId equals mdle.BatchId
                        where member.EmployeeId == employeeId && mdle.IsDeleted == false
                        orderby batch.EndDate ascending, mdle.ModuleTitle ascending
                        select mdle;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.ModuleTitle.Contains(search));
            }

            return await query.ToListAsync();
        }


    }
}