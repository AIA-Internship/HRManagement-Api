using System.Collections.Generic;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class ReadModuleDetailDto : ReadModuleDto
    {
        public string? description { get; set; }
        public DateTime createdUtcDate { get; set; }
        public List<ReadModuleContentDto> contents { get; set; } = new();
        public List<ReadQuizSummaryDto> quizzes { get; set; } = new();
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
        public decimal? latestScore { get; set; }
        public List<ReadQuizQuestionDto> questions { get; set; } = new();
    }

    public class ReadQuizQuestionDto
    {
        public int id { get; set; }
        public string text { get; set; } = null!;
        public string type { get; set; } = null!;
        public List<string> options { get; set; } = new();
    }
}
