namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class CreateQuizSubmissionDto
    {
        public int contentId { get; set; }
        public int userId { get; set; }
        public decimal score { get; set; }
    }
}
