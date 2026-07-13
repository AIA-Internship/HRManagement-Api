using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningMapping
{
    public class ProgressMapping
    {
        public static ReadProgressDto MapToReadDto(ProgressModel model)
        {
            return new ReadProgressDto
            {
                progressId = model.ProgressId,
                userId = model.EmployeeId,
                contentId = model.ProgressId,
                completedUtcDate = model.ModifiedUtcDate 
            };
        }
    }
}
