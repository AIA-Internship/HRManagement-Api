namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    using System.Collections.Generic;

    public class CreateProgramDto
    {
        public string programName { get; set; } = null!;
        public List<int> groupIds { get; set; } = new List<int>();
    }
}
