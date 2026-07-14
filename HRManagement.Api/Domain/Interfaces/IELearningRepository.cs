using HRManagement.Api.Domain.Models.Table;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Api.Domain.Interfaces
{
    public interface IELearningRepository
    {

        Task<int> CreateModuleAsync(ModuleModel entity);
        Task<bool> UpdateModuleAsync(ModuleModel entity);
        Task<bool> DeleteModuleAsync(int moduleId, string currentUserId);
        Task<ModuleModel> GetModuleByIdAsync(int moduleId);

        Task<int> AddContentAsync(ModuleContentModel entity);
        Task<IEnumerable<ModuleContentModel>> GetContentsByModuleIdAsync(int moduleId);
        Task<ModuleContentModel?> GetContentByIdAsync(int contentId);
        Task<QuizModel?> GetQuizByModuleIdAsync(int moduleId);
        Task<int> GetQuestionCountByQuizIdAsync(int quizId);
        Task<IEnumerable<QuizSubmissionModel>> GetSubmissionsByQuizIdAsync(int quizId);
        Task<IEnumerable<UserModel>> GetEligibleInternsForQuizAsync(int quizId);
        Task<bool> GradeSubmissionAsync(int submissionId, decimal score, long graderId);

        Task<QuizSubmissionModel?> GetSubmissionByIdAsync(int submissionId);
        Task<QuizModel?> GetQuizByIdAsync(int quizId);
        Task<IEnumerable<QuizQuestionModel>> GetQuestionsByQuizIdAsync(int quizId);
        Task<IEnumerable<QuizQuestionOptionModel>> GetOptionsByQuestionIdsAsync(IEnumerable<int> questionIds);
        Task<IEnumerable<StudentAnswerModel>> GetAnswersBySubmissionIdAsync(int submissionId);
        Task<UserModel?> GetUserByIdAsync(int userId);

        Task<(IEnumerable<dynamic> Interns, int TotalCount)> GetInternsPagedAsync(int targetProgramId, int pageNumber, int pageSize, string search, string role);

        Task<IEnumerable<ModuleModel>> GetAvailableModulesAsync(string role, string search);
        Task<IEnumerable<ModuleModel>> GetModulesByBatchIdAsync(int batchId, string search, IEnumerable<string> roles);
        Task<IEnumerable<ModuleModel>> GetModulesByProgramIdAsync(int programId);
        Task<IEnumerable<QuizModel>> GetQuizzesByProgramIdAsync(int programId);
        Task<IEnumerable<QuizSubmissionModel>> GetSubmissionsByUserAndQuizIdsAsync(int userId, IEnumerable<int> quizIds);
        Task<int> GetTotalModulesCountByRoleAsync(string role);
        Task<int> GetCompletedModulesCountAsync(int userId);

        Task<bool> MarkContentAsOpenedAsync(int userId, int contentId);
        Task<bool> SubmitQuizAsync(QuizSubmissionModel entity);

        Task<IEnumerable<ModuleModel>> GetModulesByEmployeeCohortAsync(int employeeId, string search);

        Task<IEnumerable<ProgressModel>> GetProgressRecordsByEmployeeAsync(int employeeId);

        Task<int> GetTotalCohortModulesCountAsync(int employeeId);

        Task<int> GetCompletedCohortModulesCountAsync(int employeeId);

        Task<IEnumerable<ModuleModel>> GetUpcomingCohortDeadlinesAsync(int employeeId, DateTime fromDateInclusive, DateTime toDateInclusive);

        Task<bool> AssignInternToGroupAsync(int employeeId, DateTime createdDate);

        Task<int> CreateProgramAsync(ProgramModel entity);
        Task<bool> GroupExistsAsync(int groupId);
        Task<IEnumerable<GroupModel>> GetAllGroupsAsync();
        Task<IEnumerable<ProgramModel>> GetAllProgramsAsync();

        Task<int> CreateBatchAsync(BatchModel entity);
        Task<bool> ProgramExistsAsync(int programId);
        Task<IEnumerable<BatchModel>> GetBatchesByProgramIdAsync(int programId);

        Task<bool> SoftDeleteModuleAsync(int moduleId, string currentUserId);
    }
}