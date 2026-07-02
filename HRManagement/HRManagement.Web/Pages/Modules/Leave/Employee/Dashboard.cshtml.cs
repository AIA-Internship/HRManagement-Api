using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Web.Pages.Modules.Leave.Employee
{
    public class HomepageModel : PageModel
    {
        public int LeaveBalance { get; set; } = 0;
        public LeaveTypeCountDto LeaveTypeCount { get; set; } = new();
        public List<LeaveRequestDto> LeaveRequests { get; set; } = new();
        public List<LeaveRequestDto> PagedRequests { get; set; } = new();
        public int PageSize { get; set; } = 5;
        public int CurrentPage { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
        public int TotalPages { get; set; } = 1;
        public int StartItem { get; set; } = 0;
        public int EndItem { get; set; } = 0;

        public Task OnGetAsync()
        {
            // Data for this page is loaded client-side using window.aiaAuth and the browser.
            // Avoid calling APIs from server since authentication token is now available only on the browser.
            LeaveBalance = 0;
            LeaveTypeCount = new LeaveTypeCountDto();
            LeaveRequests = new List<LeaveRequestDto>();
            PagedRequests = new List<LeaveRequestDto>();
            CurrentPage = 1;
            TotalItems = 0;
            TotalPages = 1;
            StartItem = 0;
            EndItem = 0;
            return Task.CompletedTask;
        }

        public IActionResult OnGetPage([FromQuery(Name = "page")] int page = 1, [FromQuery] string sort = "newest", [FromQuery] string statusOrder = null)
        {
            // Client-side now performs paging against the API directly. Keep a fallback empty response.
            return new JsonResult(new
            {
                items = new List<object>(),
                currentPage = 1,
                totalPages = 1,
                totalItems = 0,
                startItem = 0,
                endItem = 0
            });
        }
    }

    public class LeaveBalanceDto
    {
        public int EmployeeId { get; set; }
        public decimal LeaveBalance { get; set; }
    }

    public class LeaveTypeCountDto
    {
        public int PaidLeave { get; set; }
        public int UnpaidLeave { get; set; }
    }

    public class LeaveRequestDto
    {
        public int LeaveId { get; set; }
        public int RequesterId { get; set; }
        public string LeaveDescription { get; set; } = string.Empty;

        public string LeaveStatus { get; set; } = string.Empty;

        public System.DateTime LeaveStartDate { get; set; }

        public System.DateTime endDate { get; set; }

        public decimal DayAmount { get; set; }

        public string LeaveType { get; set; } = string.Empty;
        public System.DateTime CreatedUtcDate { get; set; }
    }

    public class ApiResponse<T>
    {
        public T Content { get; set; }
    }
}
