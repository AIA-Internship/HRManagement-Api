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

        // Return a human-friendly display string for LeaveType enum
        public static string LeaveTypeToDisplay(LeaveType leaveType)
        {
            var s = leaveType.ToString();
            // replace underscores, then insert spaces between camel case (e.g. PaidLeave -> Paid Leave)
            s = s.Replace("_", " ");
            s = System.Text.RegularExpressions.Regex.Replace(s, "([a-z])([A-Z])", "$1 $2");
            return s;
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