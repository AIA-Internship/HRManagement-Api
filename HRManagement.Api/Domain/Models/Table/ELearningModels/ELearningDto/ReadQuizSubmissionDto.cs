namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class ReadQuizSubmissionDto
    {
        public int submissionId { get; set; }
        public int contentId { get; set; }
        public int userId { get; set; }
        public decimal? score { get; set; }
        public DateTime submittedAt { get; set; }
    }
}
