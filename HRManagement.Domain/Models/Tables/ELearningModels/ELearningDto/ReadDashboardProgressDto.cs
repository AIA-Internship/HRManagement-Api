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
    }

    public class ReadToDoItemDto
    {
        public int moduleId { get; set; }
        public string title { get; set; } = null!;
        public DateTime? dueDate { get; set; }
        public int daysLeft { get; set; }
    }
}
