using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRManagement.Web.Pages.Modules.Timesheet
{
    public class ReviewModel : PageModel
    {
        public void OnGet(int year, int month)
        {
            // The actual data fetching is handled by the JS using fetchAPI
        }
    }
}
