
namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest
{
    public class LeaveRequestMapping
    {
        public static ReadLeaveRequestDto mapToReadDto(LeaveRequestModel model)
        {   


            return new ReadLeaveRequestDto
            {
                leaveId = model.LeaveId,
                requesterId = model.RequesterId,
                supervisorId = model.SupervisorId,
                leaveDescription = model.LeaveDescription,
                leaveStatus = leaveStatusFromInt(model.LeaveStatus),
                leaveStartDate = model.LeaveStartDate,
                dayAmount = model.DayAmount,
                leaveType = leaveTypeFromInt(model.LeaveType ?? 0),
                isCompleted = model.IsCompleted,
                isEdit = model.IsEdit,
                initialRequestId = model.InitialRequestId,
                attachmentPath = model.AttachmentPath != null
                    ? model.AttachmentPath.Split(';')
                    : null,
                createdUtcDate = model.CreatedUtcDate
            };
        }


        //helper

        private string[] splitAttachmentPath(string attachmentPath)
        {
            return attachmentPath.Split(";");
        }
        public static LeaveType leaveTypeFromInt(int num)
        {
            if (Enum.IsDefined(typeof(LeaveType), num))
            {
                return (LeaveType)num;
            }

            throw new ArgumentException("Invalid LeaveType value");
        }

        public static LeaveStatus leaveStatusFromInt(int num)
        {
            if (Enum.IsDefined(typeof(LeaveType), num))
            {
                return (LeaveStatus)num;
            }

            throw new ArgumentException("Invalid LeaveType value");
        }



    }
}