using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace HRManagement.Web.Pages.Modules.Timesheet.Supervisor
{
    public class ReviewModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public void OnGet()
        {
            if (Id == 0)
            {
                // Handle missing ID if needed, but JS will also check
            }
        }
    }
}
