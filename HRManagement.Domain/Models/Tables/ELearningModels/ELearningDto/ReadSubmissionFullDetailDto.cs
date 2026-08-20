using System;
using System.Collections.Generic;

namespace HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto
{
    public class ReadSubmissionFullDetailDto
    {
        public int submissionId { get; set; }
        public int quizId { get; set; }
        public int userId { get; set; }
        public string internName { get; set; } = null!;
        public int minimumPassingScore { get; set; }
        public int mcWeight { get; set; }
        public int essayWeight { get; set; }
        public decimal? totalScore { get; set; }
        public bool? isPassed { get; set; }
        public DateTime? gradedUtcDate { get; set; }
        public List<ReadSubmissionAnswerDetailDto> answers { get; set; } = new();
    }

    public class ReadSubmissionAnswerDetailDto
    {
        public int? answerId { get; set; }
        public int questionId { get; set; }
        public string questionText { get; set; } = null!;
        public string questionType { get; set; } = null!;
        public int sortOrder { get; set; }
        public decimal assignedScore { get; set; }
        public decimal maxScore { get; set; }
        public string? selectedOption { get; set; }
        public string? essayAnswerText { get; set; }
        public List<ReadAnswerOptionDto>? options { get; set; }
    }

    public class ReadAnswerOptionDto
    {
        public string optionLetter { get; set; } = null!;
        public string optionText { get; set; } = null!;
        public bool isCorrect { get; set; }
    }
}
