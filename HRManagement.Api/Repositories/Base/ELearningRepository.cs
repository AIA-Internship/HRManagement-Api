using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Api.Infrastructure.Repositories
{
    public class ELearningRepository : IELearningRepository
    {
        private readonly SqlDbContext _context;

        public SqlDbContext Context => _context;

        public ELearningRepository(SqlDbContext context)
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

            existing.ModuleTitle = entity.ModuleTitle;
            existing.ModuleDescription = entity.ModuleDescription;
            existing.TargetRole = entity.TargetRole;
            existing.IsPriority = entity.IsPriority;
            existing.ModifiedUtcDate = DateTime.UtcNow;

            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteModuleAsync(int moduleId)
        {
            var entity = await Context.ELearningModules.FindAsync(moduleId);
            if (entity == null) return false;

            entity.IsDeleted = true; 
            entity.ModifiedUtcDate = DateTime.UtcNow;

            await Context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> GradeSubmissionAsync(int submissionId, decimal score, long graderId)
        {
            var submission = await Context.ELearningQuizSubmissions.FindAsync(submissionId);
            if (submission == null) return false;

            submission.Score = score;
            submission.ModifiedUtcDate = DateTime.UtcNow;

            await Context.SaveChangesAsync();
            return true;
        }


        public async Task<(IEnumerable<dynamic> Interns, int TotalCount)> GetInternsPagedAsync(int pageNumber, int pageSize, string search)
        {
            var query = Context.User
                .Where(u => u.Role.RoleName == "Intern"); 

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.FullName.Contains(search));

            int totalCount = await query.CountAsync();

            var interns = await query
                .OrderBy(u => u.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    ModulesCompleted = Context.ELearningProgress
                        .Where(p => p.UserId == u.UserId && p.IsCompleted)
                        .Count(),
                    AccumulativeScore = Context.ELearningQuizSubmissions
                        .Where(s => s.UserId == u.UserId)
                        .Sum(s => s.Score ?? 0)
                })
                .ToListAsync();

            return (interns, totalCount);
        }


        public async Task<int> GetCompletedModulesCountAsync(int userId)
        {
            var modules = await Context.ELearningModules
                .Include(m => m.Contents) 
                .Where(m => !m.IsDeleted)
                .ToListAsync();

            int completedModules = 0;

            foreach (var module in modules)
            {
                var totalContentCount = module.Contents.Count();

                var completedContentCount = await Context.ELearningProgress
                    .CountAsync(p => p.UserId == userId &&
                                     p.IsCompleted &&
                                     module.Contents.Select(c => c.ContentId).Contains(p.ContentId));

                if (totalContentCount > 0 && totalContentCount == completedContentCount)
                {
                    completedModules++;
                }
            }

            return completedModules;
        }

        public async Task<int> GetTotalModulesCountByRoleAsync(string role)
        {
            return await Context.ELearningModules
                .CountAsync(m => m.TargetRole == role && !m.IsDeleted);
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

        public async Task<IEnumerable<ModuleModel>> GetAvailableModulesAsync(string role, string search)
        {
            var query = Context.ELearningModules.Where(m => !m.IsDeleted);

            if (!string.IsNullOrEmpty(role))
                query = query.Where(m => m.TargetRole == role);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(m => m.ModuleTitle.Contains(search));

            return await query.ToListAsync();
        }
        public Task<IEnumerable<QuizSubmissionModel>> GetSubmissionsByContentIdAsync(int contentId)
        {
            throw new NotImplementedException();
        }

       

        public Task<bool> MarkContentAsOpenedAsync(int userId, int contentId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SubmitQuizAsync(QuizSubmissionModel entity)
        {
            throw new NotImplementedException();
        }


       
    }
}