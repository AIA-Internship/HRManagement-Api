using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace HRManagement.Web.Pages.Modules.Timesheet.Supervisor
{
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            // The frontend JS will fetch data from /api/timesheet/supervisor/dashboard
        }
    }
}
