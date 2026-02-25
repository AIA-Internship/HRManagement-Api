namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest
{
    public class CreateLeaveRequestDto
    {
        public int RequesterId { get; set; }
        public int SupervisorId { get; set; }
        public string LeaveDescription { get; set; }
        public DateTime leaveStartDate { get; set; }
        public int DayAmount { get; set; }
        public int LeaveType { get; set; }
        public string[] AttachmentPath { get; set; }
        
    }
}
