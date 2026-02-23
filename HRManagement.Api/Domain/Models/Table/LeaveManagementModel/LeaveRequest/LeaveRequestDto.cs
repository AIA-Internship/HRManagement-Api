namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest
{
    public class LeaveRequestDto
    {
        public int? leaveId { get; set; }

        public int? requesterId { get; set; }

        public int? supervisorId { get; set; }

        public string? leaveDescription { get; set; }

        public int? leaveStatus { get; set; }

        public string? leaveStartDate { get; set; }

        public int? dayAmount { get; set; }

        public int? leaveType { get; set; }

        public DateTime? initialRequestDate { get; set; }

        public DateTime? lastUpdated { get; set; }

        public int? isDeleted { get; set; }

        public int? isCompleted { get; set; }

        public int? isEdit { get; set; }

        public int? initialRequestId { get; set; }

        public string? attachmentPath { get; set; }
    }
}