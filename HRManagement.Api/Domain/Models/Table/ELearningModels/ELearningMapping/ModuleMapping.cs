using HRManagement.Api.Domain.Models.Table.ELearningModels;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;

public class ModuleMapping
{
    public static ReadModuleDto MapToReadDto(ModuleModel model)
    {
        return new ReadModuleDto
        {
            moduleId = model.ModuleId,
            title = model.ModuleTitle,
            description = model.ModuleDescription,
            role = model.TargetRole,
            isPriority = model.IsPriority,
            createdUtcDate = model.CreatedUtcDate
        };
    }
}