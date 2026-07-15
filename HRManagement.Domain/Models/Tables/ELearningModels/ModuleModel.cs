using HRManagement.Domain.Models.Tables;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables.ELearningModels
{
    [Table("ELearningModules")]
    public class ModuleModel : BaseTableModel
    {
        [Key]
        [Column("module_id")]
        public int ModuleId { get; set; }

        [Column("batch_id")]
        public int BatchId { get; set; }

        [Column("module_title")]
        public string ModuleTitle { get; set; } = null!;

        [Column("module_description")]
        public string? ModuleDescription { get; set; }

        [Column("target_role")]
        public string TargetRole { get; set; } = null!;

        [Column("is_priority")]
        public bool IsPriority { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }
    }
}