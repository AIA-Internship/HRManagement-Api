using System.Collections.Generic;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class ReadQuizSubmissionsListDto
    {
        public int totalEligible { get; set; }
        public int submittedCount { get; set; }
        public List<ReadSubmittedItemDto> submitted { get; set; } = new();
        public List<ReadNotSubmittedItemDto> notSubmitted { get; set; } = new();
    }

    public class ReadSubmittedItemDto
    {
        public int submissionId { get; set; }
        public int userId { get; set; }
        public string name { get; set; } = null!;
        public decimal? totalScore { get; set; }
        public bool? isPassed { get; set; }
    }

    public class ReadNotSubmittedItemDto
    {
        public int userId { get; set; }
        public string name { get; set; } = null!;
    }
}
