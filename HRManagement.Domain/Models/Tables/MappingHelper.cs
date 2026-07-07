namespace HRManagement.Domain.Models.Tables
{
    public class MappingHelper
    {
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