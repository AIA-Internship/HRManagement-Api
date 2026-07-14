using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningQuizQuestionOptions")]
    public class QuizQuestionOptionModel
    {
        [Column("option_id")]
        public int OptionId { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("option_letter")]
        public string OptionLetter { get; set; } = null!; 

        [Column("option_text")]
        public string OptionText { get; set; } = null!;

        [Column("is_correct")]
        public bool IsCorrect { get; set; }
    }
}