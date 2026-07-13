using HRManagement.Api.Domain.Models.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels
{
    [Table("ELearningModules")]
    public class ModuleModel : BaseTableModel
    {
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

        public virtual ICollection<ModuleContentModel> Contents { get; set; } = new List<ModuleContentModel>();
    }
}