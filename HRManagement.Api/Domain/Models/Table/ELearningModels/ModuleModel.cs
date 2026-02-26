using HRManagement.Api.Domain.Models.Table;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels
{
    [Table("ELearningModules")]
    public class ModuleModel : BaseTableModel
    {
        [Column("module_id")]
        public int ModuleId { get; set; }

        [Column("module_title")]
        public string ModuleTitle { get; set; } = null!;

        [Column("module_description")]
        public string? ModuleDescription { get; set; }

        [Column("target_role")]
        public string? TargetRole { get; set; }

        [Column("is_priority")]
        public bool IsPriority { get; set; }

        public virtual ICollection<CreateModuleContentDto> Contents { get; set; } = new List<CreateModuleContentDto>();
    }
}