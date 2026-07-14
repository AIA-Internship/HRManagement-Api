using HRManagement.Domain.Models.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningQuizzes")]
    public class QuizModel : BaseTableModel
    {
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
    }
}