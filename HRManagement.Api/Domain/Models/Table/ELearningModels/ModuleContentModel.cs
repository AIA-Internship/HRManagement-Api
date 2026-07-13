using HRManagement.Api.Domain.Models.Table;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels
{
    [Table("ELearningModuleContents")]
    public class ModuleContentModel : BaseTableModel
    {
        [Column("content_id")]
        public int ContentId { get; set; }

        [Column("module_id")]
        public int ModuleId { get; set; }

        [Column("content_title")]
        public string ContentTitle { get; set; } = null!;

        [Column("content_type")]
        public string ContentType { get; set; } = null!; 

        [Column("content_url")]
        public string ContentUrl { get; set; } = null!; 

        [Column("sort_order")]
        public int? SortOrder { get; set; }
    }
}