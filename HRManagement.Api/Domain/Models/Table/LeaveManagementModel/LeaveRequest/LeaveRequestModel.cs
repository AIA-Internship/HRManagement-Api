using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest
{
    public class LeaveRequestModel : BaseTableModel
    {
        [Key]
        public int leaveId { get; set; }
        public int requesterId { get; set; }
        public int supervisorId { get; set; }
        public string leaveDescription { get; set; } = string.Empty;
        public int leaveStatus { get; set; }
        public string leaveStartDate { get; set; } = string.Empty;
        public int dayAmount { get; set; }
        public int? leaveType { get; set; }
        public DateTime initialRequestDate { get; set; }
        public DateTime? lastUpdated { get; set; }
        public int isDeleted { get; set; }
        public int isCompleted { get; set; }
        public int isEdit { get; set; }
        public int initialRequestId { get; set; }
        public string attachmentPath { get; set; } = string.Empty;

        public LeaveRequestModel() { }

        public LeaveRequestModel(
            int requesterId,
            int supervisorId,
            string leaveDescription,
            int leaveStatus,
            string leaveStartDate,
            int dayAmount,
            int? leaveType,
            long actioner)
        {
            this.requesterId = requesterId;
            this.supervisorId = supervisorId;
            this.leaveDescription = leaveDescription;
            this.leaveStatus = leaveStatus;
            this.leaveStartDate = leaveStartDate;
            this.dayAmount = dayAmount;
            this.leaveType = leaveType;

            initialRequestDate = DateTime.UtcNow;
            isDeleted = 0;
            isCompleted = 0;
            isEdit = 0;

            CreatedBy = actioner;
            CreatedUtcDate = DateTime.UtcNow;
            ModifiedBy = actioner;
            ModifiedUtcDate = DateTime.UtcNow;
        }
    }
}