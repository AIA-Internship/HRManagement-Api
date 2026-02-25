namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class CreateInternProfileDto
    {
        public int userId { get; set; }
        public string internRole { get; set; } = null!;
        public string internTeam { get; set; } = null!;
    }
}
