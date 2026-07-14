using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningStudentAnswers")]
    public class StudentAnswerModel
    {
        [Column("answer_id")]
        public int AnswerId { get; set; }

        [Column("submission_id")]
        public int SubmissionId { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("selected_option")]
        public string? SelectedOption { get; set; } 

        [Column("essay_answer_text")]
        public string? EssayAnswerText { get; set; } 

        [Column("assigned_score")]
        public decimal AssignedScore { get; set; } 

        [Column("is_evaluated")]
        public bool IsEvaluated { get; set; } 
    }
}