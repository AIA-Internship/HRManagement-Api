namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class ReadInternBatchStatusDto
    {
        public int employeeId { get; set; }
        public int batchId { get; set; }
        public int totalModules { get; set; }
        public int finishedModules { get; set; }
        public string status { get; set; } = "On track";
    }
}
