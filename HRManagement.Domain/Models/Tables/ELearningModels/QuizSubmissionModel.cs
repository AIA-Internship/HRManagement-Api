using HRManagement.Domain.Models.Tables;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningQuizSubmissions")]
    public class QuizSubmissionModel : BaseTableModel
    {
        [Column("submission_id")]
        public int SubmissionId { get; set; }

        [Column("quiz_id")]
        public int QuizId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; } 

        [Column("total_score")]
        public decimal? TotalScore { get; set; } 

        [Column("is_passed")]
        public bool? IsPassed { get; set; } 

        [Column("graded_by")]
        public string? GradedBy { get; set; } 

        [Column("graded_utc_date")]
        public DateTime? GradedUtcDate { get; set; }
    }
}