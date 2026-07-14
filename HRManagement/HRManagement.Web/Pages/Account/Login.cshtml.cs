using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRManagement.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // This is just a mock, the actual logic is in the JS on Login.cshtml for demonstration.
            return Page();
        }
    }
}
