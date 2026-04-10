using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRManagement.Web.Pages.Modules.Timesheet.Supervisor
{
    public class TimesheetModel : PageModel
    {
        public bool DailyLogCount { get; set; } = true;
        
        public void OnGet()
        {
            // Providing minimum baseline for JS to find elements
            DailyLogCount = true; 
        }
    }
}
