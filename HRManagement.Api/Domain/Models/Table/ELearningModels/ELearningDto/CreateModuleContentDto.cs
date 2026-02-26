namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class CreateModuleContentDto
    {
        public int moduleId { get; set; }
        public string title { get; set; } = null!;
        public bool isQuiz { get; set; }
        public string? fileName { get; set; }
        public string? filePath { get; set; }
    }
}
