using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Domain.Models.Response
{
    public class LeaveTimelineDto
    {
        public string? Status { get; set; }
        public DateTime ModifiedUtcDate { get; set; }
        public string? reason { get; set; }
    }
}
