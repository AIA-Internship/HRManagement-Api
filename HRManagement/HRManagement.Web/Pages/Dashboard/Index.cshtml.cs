using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRManagement.Web.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // For now, redirect to Login to match the "raw clone" behavior (index.html -> login.html)
            return RedirectToPage("/Account/Login");
        }
    }
}
