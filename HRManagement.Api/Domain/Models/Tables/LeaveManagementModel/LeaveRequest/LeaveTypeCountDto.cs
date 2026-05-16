namespace HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest
{
    public class LeaveTypeCountDto
    {
        public int AnnualLeave { get; set; }
        public int SickLeave { get; set; }
        public int EmergencyLeave { get; set; }
    }
}
