namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class CreateModuleContentDto
    {
        public int ModuleId { get; set; }
        public string Title { get; set; } = null!;
        public bool IsQuiz { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
    }
}
