namespace HRManagement.Domain.Models.Response
{
    public class RejectLeaveRequestDto
    {
        public int LeaveId { get; set; }
        public string? SupervisorComment { get; set; }
    }
}
