namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class ReadProgressDto
    {
        public int progressId { get; set; }
        public int userId { get; set; }
        public int contentId { get; set; }
        public bool isCompleted { get; set; }
        public DateTime? completedUtcDate { get; set; }
    }
}
