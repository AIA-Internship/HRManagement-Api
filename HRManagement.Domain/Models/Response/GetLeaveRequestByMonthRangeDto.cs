using HRManagement.Domain.Models.Tables;

namespace HRManagement.Domain.Models.Response
{
    public class GetLeaveRequestByMonthRangeDto
    {
        public int? leaveId { get; set; }
        public int requesterId { get; set; }
        public string? supervisorId { get; set; }
        public string? leaveDescription { get; set; }
        public string? leaveStatus { get; set; }
        public DateTime? leaveStartDate { get; set; }
        public decimal? dayAmount { get; set; }
        public string? leaveType { get; set; }
        public bool? isCompleted { get; set; }
        public string[]? attachmentPath { get; set; }
        public DateTime createdUtcDate { get; set; }
        public string? requesterName { get; set; }

        public GetLeaveRequestByMonthRangeDto(int leaveId, int requesterId, string? supervisorId, string? leaveDescription, int? leaveStatus, DateTime? leaveStartDate, decimal? dayAmount, int? leaveType, int? isCompleted, string? attachmentPath, DateTime createdUtcDate, string? requesterName)
        {
            this.leaveId = leaveId;
            this.requesterId = requesterId;
            this.supervisorId = supervisorId;
            this.leaveDescription = leaveDescription;
            this.leaveStatus = leaveStatus.HasValue ? MappingHelper.leaveStatusFromInt(leaveStatus.Value).ToString() : null;
            this.leaveStartDate = leaveStartDate;
            this.dayAmount = dayAmount;
            this.leaveType = leaveType.HasValue ? MappingHelper.leaveTypeFromInt(leaveType.Value).ToString() : null;
            this.isCompleted = isCompleted == 1;
            this.attachmentPath = MappingHelper.splitAttachmentPath(attachmentPath);
            this.createdUtcDate = createdUtcDate;
            this.requesterName = requesterName;
        }
    }
}
