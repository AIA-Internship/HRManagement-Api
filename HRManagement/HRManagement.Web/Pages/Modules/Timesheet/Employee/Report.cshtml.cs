using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRManagement.Web.Pages.Modules.Timesheet
{
    public class ReportModel : PageModel
    {
        // Data is fetched client-side via the API (api/timesheet/submission/history).
        // No server-side data binding needed for this page.
        public void OnGet() { }
    }
}
