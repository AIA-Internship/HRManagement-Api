namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class ReadInternQuizProgressDto
    {
        public int quizId { get; set; }
        public int moduleId { get; set; }
        public string moduleTitle { get; set; } = null!;
        public string status { get; set; } = null!;
    }
}
