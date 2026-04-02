namespace HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest
{
    public class UpdateLeaveRequestDto
    {
        public int? InitialRequestId { get; set; }
        public DateTime? LeaveStartDate { get; set; }

        public int? LeaveStatus { get; set; }

        public string? LeaveDescription { get; set; }

        public int? DayAmount { get; set; }

        public int? LeaveType { get; set; }

        public string? AttachmentPath { get; set; }
        public bool IsSupervisor { get; set; }
    }
}
