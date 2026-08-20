using System;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class ReadModuleDto
    {
        public int moduleId { get; set; }
        public string title { get; set; } = null!;
        public string? description { get; set; }
        public string role { get; set; } = null!;
        public DateTime? dueDate { get; set; }
        public string progressStatus { get; set; } = "Not Started";
    }
}