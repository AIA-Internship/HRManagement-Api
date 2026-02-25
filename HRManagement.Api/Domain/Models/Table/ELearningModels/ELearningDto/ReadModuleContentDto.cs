namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class ReadModuleContentDto
    {
        public int contentId { get; set; }
        public int moduleId { get; set; }
        public string title { get; set; } = null!;
        public bool isQuiz { get; set; }
        public string? fileName { get; set; }
        public string? filePath { get; set; }
        public int sortOrder { get; set; }
    }
}
