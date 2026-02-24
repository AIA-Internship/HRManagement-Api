namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest
{
    public class ReadLeaveRequestDto
    {
        public int? leaveId { get; set; }
        public int? requesterId { get; set; }
        public int? supervisorId { get; set; }
        public string? leaveDescription { get; set; }
        public LeaveStatus? leaveStatus { get; set; }
        public string? leaveStartDate { get; set; }
        public int? dayAmount { get; set; }
        public LeaveType? leaveType { get; set; }
        public int? isCompleted { get; set; }
        public int? isEdit { get; set; }
        public int? initialRequestId { get; set; }
        public string[]? attachmentPath { get; set; }
        public DateTime createdUtcDate { get; set; }



    }
        
}