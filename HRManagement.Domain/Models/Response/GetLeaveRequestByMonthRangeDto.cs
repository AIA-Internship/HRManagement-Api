using HRManagement.Domain.Models.Tables;

namespace HRManagement.Domain.Models.Response
{
    public class GetLeaveRequestByMonthRangeDto
    {
        public int? leaveId { get; set; }
        public int requesterId { get; set; }
        public string? requesterDisplayId { get; set; }
        public int supervisorId { get; set; }
        public string? leaveDescription { get; set; }
        public string? leaveStatus { get; set; }
        public DateTime? leaveStartDate { get; set; }
        public decimal? dayAmount { get; set; }
        public string? leaveType { get; set; }
        public bool? isCompleted { get; set; }
        public DateTime createdUtcDate { get; set; }
        public string? requesterName { get; set; }

        public GetLeaveRequestByMonthRangeDto(int leaveId, int requesterId, int supervisorId, string? leaveDescription, int? leaveStatus, DateTime? leaveStartDate, decimal? dayAmount, int? leaveType, int? isCompleted, DateTime createdUtcDate, string? requesterName, string? requesterDisplayId = null)
        {
            this.leaveId = leaveId;
            this.requesterId = requesterId;
            this.requesterDisplayId = requesterDisplayId;
            this.supervisorId = supervisorId;
            this.leaveDescription = leaveDescription;
            this.leaveStatus = leaveStatus.HasValue ? MappingHelper.leaveStatusFromInt(leaveStatus.Value).ToString() : null;
            this.leaveStartDate = leaveStartDate;
            this.dayAmount = dayAmount;
            this.leaveType = leaveType.HasValue ? MappingHelper.leaveTypeFromInt(leaveType.Value).ToString() : null;
            this.isCompleted = isCompleted == 1;
            this.createdUtcDate = createdUtcDate;
            this.requesterName = requesterName;
        }
    }
}
