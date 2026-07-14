using System;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class CreateModuleDto
    {
        public int batchId { get; set; }
        public string title { get; set; } = null!;
        public string? description { get; set; }
        public string role { get; set; } = null!;
        public bool isPriority { get; set; }
        public DateTime? dueDate { get; set; }
        public int currentUserId { get; set; }
    }
}