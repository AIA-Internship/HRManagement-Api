using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Web.Pages.Modules.Leave.Employee
{
    public class HomepageModel : PageModel
    {
        public int LeaveBalance { get; set; }
        public LeaveTypeCountDto LeaveTypeCount { get; set; } = new();

        // Full list of leave requests (source)
        public List<LeaveRequestDto> LeaveRequests { get; set; } = new();

        // Pagination properties
        public List<LeaveRequestDto> PagedRequests { get; set; } = new();
        public int PageSize { get; set; } = 5;
        public int CurrentPage { get; set; } = 1;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int StartItem { get; set; }
        public int EndItem { get; set; }

        public async Task OnGetAsync()
        {
            var token = Request.Cookies["access_token"];

            foreach (var cookie in Request.Cookies)
            {
                Console.Write($"COOKIE: {cookie.Key} = {cookie.Value}");
            }

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("TOKEN NULL / EMPTY");
            }
            else
            {
                Console.WriteLine("TOKEN: " + token);
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7089/api/");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("leave/get-leave-balance");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var result = JsonSerializer.Deserialize<ApiResponse<LeaveBalanceDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    LeaveBalance = (int)(result?.Content?.LeaveBalance ?? 0);
                }
                else
                {
                    Console.WriteLine("STATUS: " + response.StatusCode);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("API FAILED: " + response.StatusCode);
                    }
                }

                var response2 = await client.GetAsync("leave/get-all-amount-type");

                if (response2.IsSuccessStatusCode)
                {
                    var json2 = await response2.Content.ReadAsStringAsync();

                    var result2 = JsonSerializer.Deserialize<ApiResponse<LeaveTypeCountDto>>(json2, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    LeaveTypeCount = result2?.Content ?? new LeaveTypeCountDto();
                }
                else
                {
                    Console.WriteLine("TYPE COUNT FAILED: " + response2.StatusCode);
                }
            }

            // For now provide dummy data (from the PageModel) so the page only shows records that exist
            // Replace these with real data from your API as needed.
            LeaveRequests = new List<LeaveRequestDto>
            {
                new LeaveRequestDto { LeaveType = "Sick Leave", StartDate = new System.DateTime(2026,5,15), EndDate = new System.DateTime(2026,5,15), DurationDays = 1, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,5,11), EndDate = new System.DateTime(2026,5,14), DurationDays = 4, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,6,8), EndDate = new System.DateTime(2026,6,12), DurationDays = 5, Status = "Needs Approval" },
                new LeaveRequestDto { LeaveType = "Emergency Leave", StartDate = new System.DateTime(2026,4,1), EndDate = new System.DateTime(2026,4,1), DurationDays = 1, Status = "Rejected" },
                new LeaveRequestDto { LeaveType = "Sick Leave", StartDate = new System.DateTime(2026,3,2), EndDate = new System.DateTime(2026,3,2), DurationDays = 1, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,2,10), EndDate = new System.DateTime(2026,2,12), DurationDays = 3, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Emergency Leave", StartDate = new System.DateTime(2026,1,5), EndDate = new System.DateTime(2026,1,5), DurationDays = 1, Status = "Rejected" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,7,1), EndDate = new System.DateTime(2026,7,5), DurationDays = 5, Status = "Needs Approval" },
                new LeaveRequestDto { LeaveType = "Sick Leave", StartDate = new System.DateTime(2026,8,8), EndDate = new System.DateTime(2026,8,9), DurationDays = 2, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,9,15), EndDate = new System.DateTime(2026,9,17), DurationDays = 3, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,10,20), EndDate = new System.DateTime(2026,10,22), DurationDays = 3, Status = "Needs Approval" },
                new LeaveRequestDto { LeaveType = "Emergency Leave", StartDate = new System.DateTime(2026,11,11), EndDate = new System.DateTime(2026,11,11), DurationDays = 1, Status = "Rejected" }
            };

            // Read page query parameter
            var pageQuery = Request.Query["page"].ToString();
            if (!int.TryParse(pageQuery, out var page)) page = 1;
            CurrentPage = page < 1 ? 1 : page;

            // Compute pagination
            TotalItems = LeaveRequests.Count;
            TotalPages = (int)System.Math.Ceiling(TotalItems / (double)PageSize);
            if (TotalPages == 0) TotalPages = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            PagedRequests = LeaveRequests.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
            if (TotalItems == 0)
            {
                StartItem = 0;
                EndItem = 0;
            }
            else
            {
                StartItem = (CurrentPage - 1) * PageSize + 1;
                EndItem = StartItem + PagedRequests.Count - 1;
            }
        }

        // AJAX handler to return paged data as JSON for in-place table replacement
        // Return paged data from the local dummy dataset so the client-side
        // pagination can load pages without navigating away. This uses the same
        // dummy data defined in OnGetAsync.
        public async Task<IActionResult> OnGetPage([FromQuery(Name = "page")] int page = 1)
        {
            Console.WriteLine($"OnGetPage called with page={page}");
            // Create the same dummy data locally so this handler works when
            // the real API is not available (and to satisfy the user's request
            // to use dummy data stored in the PageModel file).
            var leaveRequests = new List<LeaveRequestDto>
            {
                new LeaveRequestDto { LeaveType = "Sick Leave", StartDate = new System.DateTime(2026,5,15), EndDate = new System.DateTime(2026,5,15), DurationDays = 1, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,5,11), EndDate = new System.DateTime(2026,5,14), DurationDays = 4, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,6,8), EndDate = new System.DateTime(2026,6,12), DurationDays = 5, Status = "Needs Approval" },
                new LeaveRequestDto { LeaveType = "Emergency Leave", StartDate = new System.DateTime(2026,4,1), EndDate = new System.DateTime(2026,4,1), DurationDays = 1, Status = "Rejected" },
                new LeaveRequestDto { LeaveType = "Sick Leave", StartDate = new System.DateTime(2026,3,2), EndDate = new System.DateTime(2026,3,2), DurationDays = 1, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,2,10), EndDate = new System.DateTime(2026,2,12), DurationDays = 3, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Emergency Leave", StartDate = new System.DateTime(2026,1,5), EndDate = new System.DateTime(2026,1,5), DurationDays = 1, Status = "Rejected" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,7,1), EndDate = new System.DateTime(2026,7,5), DurationDays = 5, Status = "Needs Approval" },
                new LeaveRequestDto { LeaveType = "Sick Leave", StartDate = new System.DateTime(2026,8,8), EndDate = new System.DateTime(2026,8,9), DurationDays = 2, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,9,15), EndDate = new System.DateTime(2026,9,17), DurationDays = 3, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,10,20), EndDate = new System.DateTime(2026,10,22), DurationDays = 3, Status = "Needs Approval" },
                new LeaveRequestDto { LeaveType = "Emergency Leave", StartDate = new System.DateTime(2026,11,11), EndDate = new System.DateTime(2026,11,11), DurationDays = 1, Status = "Rejected" },
                new LeaveRequestDto { LeaveType = "Sick Leave", StartDate = new System.DateTime(2026,8,8), EndDate = new System.DateTime(2026,8,9), DurationDays = 2, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,9,15), EndDate = new System.DateTime(2026,9,17), DurationDays = 3, Status = "Approved" },
                new LeaveRequestDto { LeaveType = "Personal Leave", StartDate = new System.DateTime(2026,10,20), EndDate = new System.DateTime(2026,10,22), DurationDays = 3, Status = "Needs Approval" },
                new LeaveRequestDto { LeaveType = "Emergency Leave", StartDate = new System.DateTime(2026,11,11), EndDate = new System.DateTime(2026,11,11), DurationDays = 1, Status = "Rejected" }
            };

            // PAGINATION
            var totalItems = leaveRequests.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var paged = leaveRequests.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            // Return camel-cased properties that match the client-side script expectations
            return new JsonResult(new
            {
                items = paged.Select(x => new {
                    leaveType = x.LeaveType,
                    startDate = x.StartDate.ToString("dddd, d MMMM yyyy"),
                    endDate = x.EndDate.ToString("dddd, d MMMM yyyy"),
                    duration = x.DurationDays,
                    status = x.Status
                }),
                currentPage = page,
                totalPages,
                totalItems,
                startItem = totalItems == 0 ? 0 : (page - 1) * PageSize + 1,
                endItem = totalItems == 0 ? 0 : (page - 1) * PageSize + paged.Count
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
        public int annualLeave { get; set; }
        public int sickLeave { get; set; }
        public int emergencyLeave { get; set; }
    }

    // New DTO for individual leave requests
    public class LeaveRequestDto
    {
        public string LeaveType { get; set; } = string.Empty;
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
        public string Status { get; set; } = string.Empty;
    }


    public class ApiResponse<T>
    {
        public T Content { get; set; }
    }
}
