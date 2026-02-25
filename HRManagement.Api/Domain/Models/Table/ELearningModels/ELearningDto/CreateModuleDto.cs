namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class CreateModuleDto
    {
        public string title { get; set; } = null!;
        public string? description { get; set; }
        public string role { get; set; } = null!; // Required to fix your 400 error
        public bool isPriority { get; set; }

        public long CurrentUserId { get; set; }
    }
}
