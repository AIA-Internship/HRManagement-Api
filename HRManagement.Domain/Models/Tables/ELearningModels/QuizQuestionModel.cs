using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningQuizQuestions")]
    public class QuizQuestionModel
    {
        [Key]
        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("quiz_id")]
        public int QuizId { get; set; }

        [Column("question_text")]
        public string QuestionText { get; set; } = null!;

        [Column("question_type")]
        public string QuestionType { get; set; } = null!; 

        [Column("sort_order")]
        public int SortOrder { get; set; }
    }
}