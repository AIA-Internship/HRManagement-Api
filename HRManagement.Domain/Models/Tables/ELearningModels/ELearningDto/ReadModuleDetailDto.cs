using System.Collections.Generic;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class ReadModuleDetailDto : ReadModuleDto
    {
        public string? description { get; set; }
        public DateTime createdUtcDate { get; set; }
        public List<ReadModuleContentDto> contents { get; set; } = new();
        public ReadQuizSummaryDto? quiz { get; set; }
    }

    public class ReadQuizSummaryDto
    {
        public int quizId { get; set; }
        public int questionCount { get; set; }
        public int mcCount { get; set; }
        public int essayCount { get; set; }
        public int mcWeight { get; set; }
        public int essayWeight { get; set; }
        public int minimumPassingScore { get; set; }
    }
}
