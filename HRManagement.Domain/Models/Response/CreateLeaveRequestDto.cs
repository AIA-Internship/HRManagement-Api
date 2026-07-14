namespace HRManagement.Domain.Models.Response
{
    public class CreateLeaveRequestDto
    {
        public int RequesterId { get; set; }
        public int SupervisorId { get; set; }
        public string? LeaveDescription { get; set; }
        public DateTime leaveStartDate { get; set; }
        public decimal DayAmount { get; set; }
        public int LeaveType { get; set; }
        public string? RequesterDisplayId { get; set; }
        public string? SupervisorComment { get; set; }  

    }
}
