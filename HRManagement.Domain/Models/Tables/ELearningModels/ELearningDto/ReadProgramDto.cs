namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class ReadProgramDto
    {
        public int programId { get; set; }
        public string programName { get; set; } = null!;
        public int groupId { get; set; }
    }
}
