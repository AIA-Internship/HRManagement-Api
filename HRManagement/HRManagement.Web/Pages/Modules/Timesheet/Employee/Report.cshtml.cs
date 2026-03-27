using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace HRManagement.Web.Pages.Modules.Timesheet
{
    public class ReportModel : PageModel
    {
        // PROPERTIES FOR ACTIVE SUBMISSION
        public string ActiveMonth { get; set; } = "February 2026";
        public string ActiveStatus { get; set; } = "In Review"; // In Review, Approved, Need Revision
        public string SubmissionDate { get; set; } = "March 02, 2026";
        public string RevisionComment { get; set; } = "Terdapat beberapa baris deskripsi yang kurang detail. Silakan lihat modal detail.";

        // PROPERTIES FOR HISTORY (IN PRODUCTION THIS WOULD BE DB FETCHED)
        public List<TimesheetHistoryItem> SubmissionHistory { get; set; } = new List<TimesheetHistoryItem>();

        public void OnGet()
        {
            // MOCK DATA TO POPULATE THE VIEW (SERVER SIDE)
            SubmissionHistory = new List<TimesheetHistoryItem>
            {
                new TimesheetHistoryItem { Month = "January 2026", SubmissionDate = "Feb 02, 2026", Status = "Waiting" },
                new TimesheetHistoryItem { Month = "December 2025", SubmissionDate = "Jan 03, 2026", Status = "Approved" },
                new TimesheetHistoryItem { Month = "November 2025", SubmissionDate = "Dec 02, 2025", Status = "Approved" }
            };
        }
    }

    public class TimesheetHistoryItem
    {
        public string Month { get; set; }
        public string SubmissionDate { get; set; }
        public string Status { get; set; }
        public bool CanDownload => Status == "Approved";
    }
}
