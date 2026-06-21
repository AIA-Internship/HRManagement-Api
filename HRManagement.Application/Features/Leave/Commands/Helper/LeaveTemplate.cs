using HRManagement.Domain.Models.Tables;

namespace HRManagement.Application.Features.Leave.Commands.Helper
{
    public static class LeaveTemplate
    {
        public static string NewRequestEmailSubject()
        {
            return "Leave Management - New Request";
        }

        public static string NewRequestEmailBody(LeaveRequestModel leaveRequest, Employee emp, Employee spv, string link)
        {
            return $@"
            Dear {spv.FullName},

            {emp.FullName} has submitted a leave request on {leaveRequest.CreatedUtcDate}

            Leave ID: {leaveRequest.LeaveId}
            Category: {MappingHelper.leaveTypeFromInt((int)leaveRequest!.LeaveType).ToString()}
            Date: {leaveRequest.LeaveStartDate} - {leaveRequest.LeaveStartDate.AddDays((double)leaveRequest.DayAmount)}

            Please review and approve the request using the link below.
            {link}

            Thank you.

            This is a system-generated message, do not reply to this message
            ";
        }

        public static string ApprovedEmailSubject(string employeeName, string supervisorName)
        {
            return "";
        }

        public static string ApprovedEmailBody(string employeeName, string supervisorName)
        {
            return "";
        }

        public static string RejectedEmailSubject(string employeeName, string supervisorName)
        {
            return "";
        }

        public static string RejectedEmailBody(string employeeName, string supervisorName)
        {
            return "";
        }

        public static string ReminderEmailSubject()
        {
            return $"Leave Management - leave request needs your attention";
        }

        public static string ReminderEmailBody(LeaveRequestModel leaveRequest, Employee emp, Employee spv, string link)
        {
            return $@"
            Leave Management Reminder

            Dear {spv.FullName},

            {emp.FullName} has submitted a leave request on {leaveRequest.CreatedUtcDate} and the start date is within 48 hours.

            Leave ID: {leaveRequest.LeaveId}
            Category: {MappingHelper.leaveTypeFromInt((int)leaveRequest!.LeaveType).ToString()}
            Date: {leaveRequest.LeaveStartDate} - {leaveRequest.LeaveStartDate.AddDays((double)leaveRequest.DayAmount)}

            Please review and approve the request using the link below:
            {link}

            Thank you.

            This is a system-generated message, do not reply to this message.
            ";
        }
    }
}