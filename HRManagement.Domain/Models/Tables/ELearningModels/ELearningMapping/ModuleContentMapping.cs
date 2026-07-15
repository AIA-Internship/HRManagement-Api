using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningMapping
{
    public class ModuleContentMapping
    {
        public static ReadModuleContentDto MapToReadDto(ModuleContentModel model)
        {
            return new ReadModuleContentDto
            {
                contentId = model.ContentId,
                moduleId = model.ModuleId,
                title = model.ContentTitle,
                isQuiz = false,
                fileName = model.ContentUrl,
                filePath = model.ContentUrl,
                sortOrder = model.SortOrder ?? 0
            };
        }
    }
}
