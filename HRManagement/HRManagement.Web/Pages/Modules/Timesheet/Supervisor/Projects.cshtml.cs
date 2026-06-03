using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRManagement.Web.Pages.Modules.Timesheet.Supervisor
{
    public class ProjectsModel : PageModel
    {
        public void OnGet()
        {
            // Project data is loaded via JS from /api/master/projects
        }
    }
}
