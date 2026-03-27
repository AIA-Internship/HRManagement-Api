using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace HRManagement.Web.Pages.Modules.Timesheet
{
    public class WeeklyProjectLog
    {
        public string Name { get; set; } = "";
        public string Mon { get; set; } = "00h : 00m";
        public string Tue { get; set; } = "00h : 00m";
        public string Wed { get; set; } = "00h : 00m";
        public string Thu { get; set; } = "00h : 00m";
        public string Fri { get; set; } = "00h : 00m";
        public string Sat { get; set; } = "-";
        public string Sun { get; set; } = "-";
        public string Total { get; set; } = "00h : 00m";
    }

    public class ManagementModel : PageModel
    {
        public int DailyLogCount { get; set; } = 0;
        public string DailyLogErrorMessage { get; set; } = "No activities recorded for this date. Please log your tasks to proceed.";
        
        // This is the property that the error was complaining about
        public List<WeeklyProjectLog> WeeklyLogs { get; set; } = new List<WeeklyProjectLog>();

        public void OnGet()
        {
            // Initializing with zero-hour project entries
            WeeklyLogs = new List<WeeklyProjectLog>
            {
                new WeeklyProjectLog { Name = "Insurable Interest Project" },
                new WeeklyProjectLog { Name = "Click Revamp Enhancement" },
                new WeeklyProjectLog { Name = "BAU Huddle 2.0 Project" },
                new WeeklyProjectLog { Name = "iRecruit 3.0 Module Release" },
                new WeeklyProjectLog { Name = "Upfront Validation Project" },
                new WeeklyProjectLog { Name = "APL 2.0 - AIA Signature" }
            };

            DailyLogCount = 0; 
        }
    }
}
