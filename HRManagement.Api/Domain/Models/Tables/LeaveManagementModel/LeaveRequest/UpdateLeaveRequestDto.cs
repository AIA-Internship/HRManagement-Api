namespace HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest
{
    public class UpdateLeaveRequestDto
    {
        public int? InitialRequestId { get; set; } = -1;
        public DateTime? LeaveStartDate { get; set; } = DateTime.Now;

        public int? LeaveStatus { get; set; } = 1;

        public string? LeaveDescription { get; set; } = "";

        public decimal? DayAmount { get; set; } = 1;

        public int? LeaveType { get; set; } = 1;

        public string[]? AttachmentPath { get; set; } = {""};
        public bool IsSupervisor { get; set; } = true;
    }
}
