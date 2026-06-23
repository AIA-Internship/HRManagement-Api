namespace HRManagement.Domain.Models.Response
{
    public class LeaveTypeCountDto
    {
        public int PaidLeave { get; set; }
        public int UnpaidLeave { get; set; }


        public static LeaveTypeCountDto empty()
        {
            return new LeaveTypeCountDto
            {
                PaidLeave = 0,
                UnpaidLeave = 0
            };
        }
    }
}
