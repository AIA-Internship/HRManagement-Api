using System;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto
{
    public class CreateBatchDto
    {
        public int programId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
