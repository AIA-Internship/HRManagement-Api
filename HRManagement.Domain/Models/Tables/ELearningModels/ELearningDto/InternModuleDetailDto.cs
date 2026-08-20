using System;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class InternModuleDetailDto
    {
        public int ModuleId { get; set; }
        public string Title { get; set; } = null!;
        public int BatchId { get; set; }
        public string BatchName { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public string ProgressStatus { get; set; } = "Not Started";
        public decimal? Score { get; set; }
    }
}
