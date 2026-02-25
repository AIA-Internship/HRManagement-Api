using HRManagement.Api.Domain.Models.Table;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels
{
    [Table("ELearningQuizSubmissions")]
    public class QuizSubmissionModel : BaseTableModel
    {
        [Column("submission_id")]
        public int SubmissionId { get; set; }

        [Column("content_id")]
        public int ContentId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("answer_original_file_name")]
        public string AnswerOriginalFileName { get; set; } = null!;

        [Column("answer_stored_file_name")]
        public string AnswerStoredFileName { get; set; } = null!;

        [Column("answer_file_path")]
        public string AnswerFilePath { get; set; } = null!;

        [Column("submitted_utc_date")]
        public DateTime SubmittedUtcDate { get; set; }

        [Column("score")]
        public decimal? Score { get; set; }

        [Column("graded_utc_date")] // Matches image_620a11.png
        public DateTime? GradedUtcDate { get; set; }
    }
}