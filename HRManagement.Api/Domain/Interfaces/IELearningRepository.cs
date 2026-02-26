using HRManagement.Api.Domain.Models.Table.ELearningModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Api.Domain.Interfaces
{
    public interface IELearningRepository
    {
       
        Task<int> CreateModuleAsync(ModuleModel entity);
        Task<bool> UpdateModuleAsync(ModuleModel entity);
        Task<bool> DeleteModuleAsync(int moduleId);
        Task<ModuleModel> GetModuleByIdAsync(int moduleId);

        Task<int> AddContentAsync(CreateModuleContentDto entity);
        Task<IEnumerable<CreateModuleContentDto>> GetContentsByModuleIdAsync(int moduleId);
        Task<IEnumerable<QuizSubmissionModel>> GetSubmissionsByContentIdAsync(int contentId);
        Task<bool> GradeSubmissionAsync(int submissionId, decimal score, long graderId);

        Task<(IEnumerable<dynamic> Interns, int TotalCount)> GetInternsPagedAsync(int pageNumber, int pageSize, string search);
     
        Task<IEnumerable<ModuleModel>> GetAvailableModulesAsync(string role, string search);
        Task<int> GetTotalModulesCountByRoleAsync(string role);
        Task<int> GetCompletedModulesCountAsync(int userId);

        Task<bool> MarkContentAsOpenedAsync(int userId, int contentId);
        Task<bool> SubmitQuizAsync(QuizSubmissionModel entity);
    }
}