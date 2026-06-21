using HRManagement.Domain.Models.Tables;
using MimeKit;

namespace HRManagement.Application.Features.Leave.Commands
{
    public class LeaveEmailTemplate
    {
        // 1. Request Approval Notification to SPV
        public static TextPart GetRequestApprovalToSpv(
            string supervisorName,
            string internName,
            DateTime requestDate,
            int leaveType,
            DateTime startDate,
            DateTime endDate,
            string redirectLink)
        {

            var type = MappingHelper.leaveTypeFromInt(leaveType).ToString();
            return new TextPart("html")
            {
                Text = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <p>Dear {supervisorName},</p>

                <p>
                    <b>{internName}</b> has submitted a leave request on 
                    <b>{requestDate:dd MMM yyyy}</b>.
                </p>

                <p>
                    <b>Category:</b> {type}<br/>
                    <b>Date:</b> {startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}
                </p>

                <p>Please review and approve the request using the link below.</p>

                <p>
                    <a href='{redirectLink}' style='
                        display:inline-block;
                        padding:10px 15px;
                        background-color:#007bff;
                        color:white;
                        text-decoration:none;
                        border-radius:5px;
                    '>
                        Open Leave Request Approval Page
                    </a>
                </p>

                <p>Thank you.</p>

                <hr/>
                <p style='font-size:12px;color:gray;'>
                    This is a system-generated message, do not reply to this message.
                </p>
            </body>
            </html>"
            };
        }

        // 2. Reminder Approval Notification to SPV
        public static TextPart GetReminderApprovalToSpv(
            string supervisorName,
            string internName,
            DateTime requestDate,
            string leaveId,
            string leaveType,
            DateTime startDate,
            DateTime endDate,
            string redirectLink)
        {
            return new TextPart("html")
            {
                Text = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <h3>Leave Management Reminder</h3>

                <p>Dear {supervisorName},</p>

                <p>
                    <b>{internName}</b> has submitted a leave request on 
                    <b>{requestDate:dd MMM yyyy}</b> and the start date is within 48 hours.
                </p>

                <p>
                    <b>Leave ID:</b> {leaveId}<br/>
                    <b>Category:</b> {leaveType}<br/>
                    <b>Date:</b> {startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}
                </p>

                <p>Please review and approve the request using the link below:</p>

                <p>
                    <a href='{redirectLink}' style='
                        display:inline-block;
                        padding:10px 15px;
                        background-color:#ffc107;
                        color:black;
                        text-decoration:none;
                        border-radius:5px;
                    '>
                        Open Leave Request Approval Page
                    </a>
                </p>

                <p>Thank you.</p>

                <hr/>
                <p style='font-size:12px;color:gray;'>
                    This is a system-generated message, do not reply to this message.
                </p>
            </body>
            </html>"
            };
        }

        // 3. Approved Notification to Intern
        public static TextPart GetApprovedEmailBody(
            string fullname,
            DateTime startDate,
            string redirectLink)
        {
            return new TextPart("html")
            {
                Text = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <p>Dear {fullname},</p>

                <p>
                    Your leave request for 
                    <b>{startDate:dd MMM yyyy}</b> has been 
                    <b style='color: green;'>approved</b> by your Supervisor.
                </p>

                <p>You can review the approved request in your leave dashboard.</p>

                <p>
                    <a href='{redirectLink}' style='
                        display:inline-block;
                        padding:10px 15px;
                        background-color:#dc3545;
                        color:white;
                        text-decoration:none;
                        border-radius:5px;
                    '>
                        View Leave Request Dashboard
                    </a>
                </p>

                <p>Thank you.</p>

                <hr/>
                <p style='font-size:12px;color:gray;'>
                    This is a system-generated message, do not reply to this message.
                </p>
            </body>
            </html>"
            };
        }

        // 4. Rejected Notification to Intern
        public static TextPart GetRejectedEmailBody(
            string fullname,
            DateTime startDate,
            string redirectLink)
        {
            return new TextPart("html")
            {
                Text = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <p>Dear {fullname},</p>

                <p>
                    Your leave request for 
                    <b>{startDate:dd MMM yyyy}</b> has been 
                    <b style='color: red;'>rejected</b> by your Supervisor.
                </p>

                <p>You can review the rejected request in your leave dashboard.</p>

                <p>
                    <a href='{redirectLink}' style='
                        display:inline-block;
                        padding:10px 15px;
                        background-color:#dc3545;
                        color:white;
                        text-decoration:none;
                        border-radius:5px;
                    '>
                        View Leave Request Dashboard
                    </a>
                </p>

                <p>Thank you.</p>

                <hr/>
                <p style='font-size:12px;color:gray;'>
                    This is a system-generated message, do not reply to this message.
                </p>
            </body>
            </html>"
            };
        }


        //subject
        public static string getApprovedEmailSubject()
        {
            return "Leave Request approved";
        }

        public static string getRejectedEmailSubject()
        {
            return "Leave Request rejected";
        }

        public static string GetRequestApprovalToSpvSubject()
        {
            return "Request Leave Approval";
        }


    }
}