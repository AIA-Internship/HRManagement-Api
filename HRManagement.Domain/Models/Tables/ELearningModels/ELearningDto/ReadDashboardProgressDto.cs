using System;
using System.Collections.Generic;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class ReadDashboardProgressDto
    {
        public int totalModules { get; set; }
        public int completedModules { get; set; }
        public string displayString { get; set; } = null!;
        public List<ReadToDoItemDto> toDoList { get; set; } = new();
        public List<ReadDashboardBatchDto> batches { get; set; } = new();
    }

    public class ReadDashboardBatchDto
    {
        public int id { get; set; }
        public string name { get; set; } = null!;
        public string period { get; set; } = null!;
        public int endsIn { get; set; }
        public string status { get; set; } = null!;
    }

    public class ReadToDoItemDto
    {
        public int moduleId { get; set; }
        public string title { get; set; } = null!;
        public DateTime? dueDate { get; set; }
        public int daysLeft { get; set; }
    }
}
