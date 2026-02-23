using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel
{
    [Table("leave_response")]
    public class LeaveResponse
    {
        [Key]
        [Column("response_id")]
        public int ResponseId { get; set; }

        [MaxLength(255)]
        [Column("response_description")]
        public string? ResponseDescription { get; set; }

        [Column("response_datetime")]
        public DateTime? ResponseDatetime { get; set; }

        [Required]
        [Column("is_deleted")]
        public int IsDeleted { get; set; }

        [Required]
        [Column("is_approval")]
        public int IsApproval { get; set; }
    }
}
