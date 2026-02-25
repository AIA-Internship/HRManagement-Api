using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningMapping
{
    public class QuizSubmissionMapping
    {
        public static ReadQuizSubmissionDto MapToReadDto(QuizSubmissionModel model)
        {
            return new ReadQuizSubmissionDto
            {
                submissionId = model.SubmissionId,
                contentId = model.ContentId,
                userId = model.UserId,
                score = model.Score?? 0m,
                submittedAt = model.CreatedUtcDate
            };
        }
    }
}
