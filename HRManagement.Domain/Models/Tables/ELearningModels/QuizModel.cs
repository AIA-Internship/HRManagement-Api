using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningQuizzes")]
    public class QuizModel
    {
        public QuizModel()
        {
            IsDeleted = false;
            CreatedBy = "0";
            CreatedUtcDate = DateTime.UtcNow;
        }

        [Key]
        [Column("quiz_id")]
        public int QuizId { get; set; }

        [Column("module_id")]
        public int ModuleId { get; set; }

        [Column("mc_count")]
        public int McCount { get; set; }

        [Column("essay_count")]
        public int EssayCount { get; set; }

        [Column("mc_weight")]
        public int McWeight { get; set; }

        [Column("essay_weight")]
        public int EssayWeight { get; set; }

        [Column("minimum_passing_score")]
        public int MinimumPassingScore { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("CreatedUtcDate")]
        public DateTime CreatedUtcDate { get; set; }
    }
}