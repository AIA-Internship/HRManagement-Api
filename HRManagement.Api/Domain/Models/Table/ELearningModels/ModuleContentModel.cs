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

        [Column("is_quiz")]
        public bool IsQuiz { get; set; }

        [Column("original_file_name")]
        public string OriginalFileName { get; set; } = null!;

        [Column("stored_file_name")]
        public string StoredFileName { get; set; } = null!;

        [Column("file_path")]
        public string FilePath { get; set; } = null!;

        [Column("file_ext")]
        public string? FileExt { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; }

        public virtual ModuleModel Module { get; set; } = null!;
    }
}