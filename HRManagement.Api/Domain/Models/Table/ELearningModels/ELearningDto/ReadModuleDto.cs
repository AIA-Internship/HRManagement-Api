namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class ReadModuleDto
    {
        public int moduleId { get; set; }
        public string title { get; set; } = null!;
        public string? description { get; set; }
        public string? role { get; set; }
        public bool isPriority { get; set; }
        public DateTime createdUtcDate { get; set; }
    }
}
