using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;

public partial class    LeaveRequestModel
{
    [Key]
    [Column("leave_id")]
    public int LeaveId { get; set; }

    [Column("requester_id")]
    public int RequesterId { get; set; }

    [Column("supervisor_id")]
    public int SupervisorId { get; set; }

    [Column("leave_description")]
    public string LeaveDescription { get; set; } = null!;

    [Column("leave_status")]
    public int LeaveStatus { get; set; }

    [Column("leave_start_date")]
    public DateTime LeaveStartDate { get; set; }

    [Column("day_amount")]
    public int DayAmount { get; set; }

    [Column("leave_type")]
    public int? LeaveType { get; set; }

    [Column("is_deleted")]
    public int IsDeleted { get; set; }

    [Column("is_completed")]
    public int IsCompleted { get; set; }

    [Column("is_edit")]
    public int IsEdit { get; set; }

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
}
