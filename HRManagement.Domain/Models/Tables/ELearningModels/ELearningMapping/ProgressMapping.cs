using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningMapping
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
