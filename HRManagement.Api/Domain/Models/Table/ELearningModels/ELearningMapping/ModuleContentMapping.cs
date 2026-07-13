using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningMapping
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
                filePath = $"/elearning/{model.ContentUrl}",
                sortOrder = model.SortOrder ?? 0
            };
        }
    }
}
