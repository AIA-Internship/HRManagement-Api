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
                submittedAt = model.CreatedUtcDate
            };
        }
    }
}
