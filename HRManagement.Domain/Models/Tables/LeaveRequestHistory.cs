using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Domain.Models.Tables
{
    public partial class LeaveRequestHistory
    {
        [Key]
        [Column("leave_id")]
        public int LeaveId { get; set; }

        [Column("requester_id")]
        public int RequesterId { get; set; }

        [Column("supervisor_id")]
        public string? SupervisorId { get; set; }

        [Column("leave_description")]
        public string LeaveDescription { get; set; } = null!;

        [Column("leave_status")]
        public int LeaveStatus { get; set; }

        [Column("leave_start_date")]
        public DateTime LeaveStartDate { get; set; }

        [Column("day_amount")]
        public decimal DayAmount { get; set; }

        [Column("leave_type")]
        public int? LeaveType { get; set; }

        [Column("is_deleted")]
        public int IsDeleted { get; set; }

        [Column("is_completed")]
        public int IsCompleted { get; set; }

        [Column("initial_request_id")]
        public int InitialRequestId { get; set; }


        [Column("attachment_path")]
        public string? AttachmentPath { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_utc_date")]
        public DateTime CreatedUtcDate { get; set; }

        [Column("modified_by")]
        public int ModifiedBy { get; set; }

        [Column("modified_utc_date")]
        public DateTime ModifiedUtcDate { get; set; }

        [Column("requester_display_id")]
        public string? RequesterDisplayId { get; set; }

    }
}
