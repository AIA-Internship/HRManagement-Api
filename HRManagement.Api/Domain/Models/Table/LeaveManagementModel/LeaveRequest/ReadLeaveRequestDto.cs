namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest
{
    public class ReadLeaveRequestDto
    {
        public int? leaveId { get; set; }
        public int? requesterId { get; set; }
        public int? supervisorId { get; set; }
        public string? leaveDescription { get; set; }
        public string? leaveStatus { get; set; }
        public DateTime? leaveStartDate { get; set; }
        public int? dayAmount { get; set; }
        public string? leaveType { get; set; }
        public bool? isCompleted { get; set; }
        public bool? isEdit { get; set; }
        public int? initialRequestId { get; set; }
        public string[]? attachmentPath { get; set; }
        public DateTime createdUtcDate { get; set; }



    }
        
}