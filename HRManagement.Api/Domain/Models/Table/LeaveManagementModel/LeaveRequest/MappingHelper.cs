
namespace HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest
{
    public class MappingHelper
    {
        public static string[] splitAttachmentPath(string attachmentPath)
        {
            return attachmentPath.Split(";");
        }

        public static string joinAttachmentPath(string[] attachmentPaths)
        {
            return string.Join(";", attachmentPaths);
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
            if (Enum.IsDefined(typeof(LeaveStatus), num))
            {
                return (LeaveStatus)num;
            }

            throw new ArgumentException("Invalid LeaveType value");
        }


    }
}