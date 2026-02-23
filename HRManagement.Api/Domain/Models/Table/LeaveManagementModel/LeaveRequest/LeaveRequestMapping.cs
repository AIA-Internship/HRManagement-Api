namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest
{
    public class LeaveRequestMapping
    {
            public static LeaveRequestModel MapToLeaveRequest(LeaveRequestDto updateLeaveRequestDto)
            {
            return new LeaveRequestModel
            {
                leaveId = updateLeaveRequestDto.leaveId ?? 0,
                supervisorId = updateLeaveRequestDto.supervisorId ?? 0,
                leaveDescription = updateLeaveRequestDto.leaveDescription ?? string.Empty,
                leaveStatus = updateLeaveRequestDto.leaveStatus ?? 0,
                leaveStartDate = updateLeaveRequestDto.leaveStartDate ?? string.Empty,
                dayAmount = updateLeaveRequestDto.dayAmount ?? 0,
                leaveType = updateLeaveRequestDto.leaveType ?? 0,
                isCompleted = updateLeaveRequestDto.isCompleted ?? 0,
                isEdit = updateLeaveRequestDto.isEdit ?? 0
            };
            }
    
            public static void MapToExistingLeaveRequest(LeaveRequestModel existingLeaveRequest, LeaveRequestDto updateLeaveRequestDto)
            {
                if (updateLeaveRequestDto.supervisorId.HasValue)
                {
                    existingLeaveRequest.supervisorId = updateLeaveRequestDto.supervisorId.Value;
                }
                if (!string.IsNullOrEmpty(updateLeaveRequestDto.leaveDescription))
                {
                    existingLeaveRequest.leaveDescription = updateLeaveRequestDto.leaveDescription;
                }
                if (updateLeaveRequestDto.leaveStatus.HasValue)
                {
                    existingLeaveRequest.leaveStatus = updateLeaveRequestDto.leaveStatus.Value;
                }
                if (!string.IsNullOrEmpty(updateLeaveRequestDto.leaveStartDate))
                {
                    existingLeaveRequest.leaveStartDate = updateLeaveRequestDto.leaveStartDate;
                }
                if (updateLeaveRequestDto.dayAmount.HasValue)
                {
                    existingLeaveRequest.dayAmount= updateLeaveRequestDto.dayAmount.Value;
                }
                if (updateLeaveRequestDto.leaveType.HasValue)
                {
                    existingLeaveRequest.leaveType = updateLeaveRequestDto.leaveType.Value;
                }
                if (updateLeaveRequestDto.isCompleted.HasValue)
                {
                    existingLeaveRequest.isCompleted = updateLeaveRequestDto.isCompleted.Value;
                }
                if (updateLeaveRequestDto.isEdit.HasValue)
                {
                    existingLeaveRequest.isEdit = updateLeaveRequestDto.isEdit.Value;
                }
        }
    }
}
