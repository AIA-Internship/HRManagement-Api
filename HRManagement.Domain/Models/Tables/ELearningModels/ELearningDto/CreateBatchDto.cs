using System;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class CreateBatchDto
    {
        public int programId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
