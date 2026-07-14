using System;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class ReadBatchDto
    {
        public int batchId { get; set; }
        public int programId { get; set; }
        public string batchName { get; set; } = null!;
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
