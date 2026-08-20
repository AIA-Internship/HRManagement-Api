using HRManagement.Domain.Models.Tables.ELearningModels;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;

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
            dueDate = model.DueDate,
            batchId = model.BatchId
        };
    }
}