using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningMapping
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
