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
                userId = model.UserId,
                contentId = model.ContentId,
                isCompleted = model.IsCompleted,
                completedUtcDate = model.ModifiedUtcDate // Uses audit date as completion date
            };
        }
    }
}
