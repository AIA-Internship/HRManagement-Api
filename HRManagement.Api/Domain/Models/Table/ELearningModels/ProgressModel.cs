using HRManagement.Api.Domain.Models.Table;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.ELearningModels
{
    [Table("ELearningProgress")]
    public class ProgressModel : BaseTableModel
    {
        [Column("progress_id")]
        public int ProgressId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("content_id")]
        public int ContentId { get; set; }

        [Column("is_opened")]
        public bool IsOpened { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; }
    }
}