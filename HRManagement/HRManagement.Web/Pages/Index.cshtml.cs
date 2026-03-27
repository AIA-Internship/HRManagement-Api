using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRManagement.Web.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirect to Login to ensure consistent entry flow.
            return RedirectToPage("/Account/Login");
        }
    }
}
