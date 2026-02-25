namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class CreateProgressDto
    {
        public int userId { get; set; }
        public int contentId { get; set; }
        public bool isCompleted { get; set; }
    }
}
