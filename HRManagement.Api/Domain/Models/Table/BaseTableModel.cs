using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table
{
    public class BaseTableModel
    {
        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("CreatedUtcDate")]
        public DateTime CreatedUtcDate { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("ModifiedUtcDate")]
        public DateTime? ModifiedUtcDate { get; set; }

        public BaseTableModel()
        {
            IsDeleted = false;
            CreatedUtcDate = DateTime.UtcNow;
            ModifiedUtcDate = DateTime.UtcNow;
            CreatedBy = "0";
            ModifiedBy = "0";
        }
    }
}
