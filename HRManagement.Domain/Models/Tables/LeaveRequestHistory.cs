using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables
{
    public partial class LeaveRequestHistory
    {
        [Key]
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("leave_id")]
        public int LeaveId { get; set; }

        [Column("modified_utc_date")]
        public DateTime ModifiedUtcDate { get; set; }

    }
}
