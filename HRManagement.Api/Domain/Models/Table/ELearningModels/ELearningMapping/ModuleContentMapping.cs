using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningMapping
{
    public class ModuleContentMapping
    {
        public static ReadModuleContentDto MapToReadDto(CreateModuleContentDto model)
        {
            return new ReadModuleContentDto
            {
                contentId = model.ContentId,
                moduleId = model.ModuleId,
                title = model.ContentTitle,
                isQuiz = model.IsQuiz,
                fileName = model.StoredFileName,
                filePath = model.FilePath
            };
        }

        public static CreateModuleContentDto MapToModel(ELearningDto.CreateModuleContentDto dto, long userId)
        {
            return new CreateModuleContentDto
            {
                ModuleId = dto.ModuleId,
                ContentTitle = dto.Title,
                IsQuiz = dto.IsQuiz,
                OriginalFileName = dto.FileName ?? "",
                StoredFileName = Guid.NewGuid().ToString() +
                    Path.GetExtension(dto.FileName ?? ""),
                FilePath = dto.FilePath ?? "",
                FileExt = Path.GetExtension(dto.FileName ?? ""),
                CreatedBy = userId.ToString(),
                CreatedUtcDate = DateTime.UtcNow
            };
        }
    }
}
