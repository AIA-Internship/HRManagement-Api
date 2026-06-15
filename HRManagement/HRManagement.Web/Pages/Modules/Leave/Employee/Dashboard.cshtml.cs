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
        public List<LeaveRequestDto> LeaveRequests { get; set; } = new();
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

            int requesterId = 1;
            var reqQuery = Request.Query["requesterId"].ToString();
            if (!int.TryParse(reqQuery, out requesterId))
            {
                var claim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User?.FindFirst("sub");
                if (claim != null && int.TryParse(claim.Value, out var cid)) requesterId = cid;
            }

            using (var client3 = new HttpClient())
            {
                client3.BaseAddress = new Uri("https://localhost:7089/api/");
                client3.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                try
                {
                    var resp = await client3.GetAsync($"leave/get-by-requester-id?max=1000");
                    if (resp.IsSuccessStatusCode)
                    {
                        var js = await resp.Content.ReadAsStringAsync();
                        var r = JsonSerializer.Deserialize<ApiResponse<List<LeaveRequestDto>>>(js, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        LeaveRequests = r?.Content ?? new List<LeaveRequestDto>();
                    }
                    else
                    {
                        Console.WriteLine("Failed to fetch leave requests: " + resp.StatusCode);
                        LeaveRequests = new List<LeaveRequestDto>();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching leave requests: " + ex.Message);
                    LeaveRequests = new List<LeaveRequestDto>();
                }
            }

            var pageQuery = Request.Query["page"].ToString();
            if (!int.TryParse(pageQuery, out var page)) page = 1;
            CurrentPage = page < 1 ? 1 : page;

            var sort = Request.Query["sort"].ToString();
            var statusOrderQuery = Request.Query["statusOrder"].ToString();

            var statusPriority = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(statusOrderQuery))
            {
                var parts = statusOrderQuery.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                for (int i = 0; i < parts.Count; i++) statusPriority[parts[i]] = i;
            }

            if (statusPriority.Count == 0)
            {
                statusPriority["1"] = 0; // Needs Approval
                statusPriority["2"] = 1; // Approved
                statusPriority["3"] = 2; // Rejected
            }

            Func<LeaveRequestDto, int> getPriority = (lr) =>
            {
                var s = (lr.LeaveStatus ?? "").Trim().ToLowerInvariant();

                string normalized = s switch
                {
                    "1" => "1",
                    "2" => "2",
                    "3" => "3",

                    "needs approval" => "1",
                    "need approval" => "1",
                    "pending" => "1",

                    "approved" => "2",

                    "rejected" => "3",

                    _ => "999"
                };

                return statusPriority.TryGetValue(normalized, out var p)
                    ? p
                    : int.MaxValue;
            };

            var isOldest = sort?.ToLowerInvariant() == "oldest";

            LeaveRequests = isOldest
                ? LeaveRequests.OrderBy(getPriority).ThenBy(x => x.CreatedUtcDate).ToList()
                : LeaveRequests.OrderBy(getPriority).ThenByDescending(x => x.CreatedUtcDate).ToList();

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

        public async Task<IActionResult> OnGetPage([FromQuery(Name = "page")] int page = 1, [FromQuery] string sort = "newest", [FromQuery] string statusOrder = null)
        {
            Console.WriteLine($"OnGetPage called with page={page}");

            var token = Request.Cookies["access_token"];
            int requesterId = 1;
            var rq = Request.Query["requesterId"].ToString();
            if (!int.TryParse(rq, out requesterId))
            {
                var claim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User?.FindFirst("sub");
                if (claim != null && int.TryParse(claim.Value, out var cid)) requesterId = cid;
            }

            List<LeaveRequestDto> leaveRequests;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7089/api/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                try
                {
                    var resp = await client.GetAsync($"leave/get-by-requester-id?max=1000");
                    if (!resp.IsSuccessStatusCode)
                    {
                        leaveRequests = new List<LeaveRequestDto>();
                    }
                    else
                    {
                        var js = await resp.Content.ReadAsStringAsync();
                        var r = JsonSerializer.Deserialize<ApiResponse<List<LeaveRequestDto>>>(js, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        leaveRequests = r?.Content ?? new List<LeaveRequestDto>();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("OnGetPage error: " + ex.Message);
                    leaveRequests = new List<LeaveRequestDto>();
                }
            }

            // PAGINATION
            var totalItems = leaveRequests.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var statusPriority = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(statusOrder))
            {
                var parts = statusOrder.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                for (int i = 0; i < parts.Count; i++) statusPriority[parts[i]] = i;
            }
            if (statusPriority.Count == 0)
            {
                statusPriority["1"] = 0;
                statusPriority["2"] = 1;
                statusPriority["3"] = 2;
            }

            Func<LeaveRequestDto, int> getPriority = (lr) =>
            {
                var s = lr.LeaveStatus ?? "1";
                if (statusPriority.TryGetValue(s, out var p)) return p;
                var sl = (s ?? string.Empty).ToLowerInvariant();
                if (sl.Contains("approve") && statusPriority.TryGetValue("2", out var p2)) return p2;
                if (sl.Contains("reject") && statusPriority.TryGetValue("3", out var p3)) return p3;
                if ((sl.Contains("need") || sl.Contains("approval")) && statusPriority.TryGetValue("1", out var p1)) return p1;
                return int.MaxValue;
            };

            var isOldest = sort?.ToLowerInvariant() == "oldest";

            leaveRequests = isOldest
                ? leaveRequests.OrderBy(getPriority).ThenBy(x => x.CreatedUtcDate).ToList()
                : leaveRequests.OrderBy(getPriority).ThenByDescending(x => x.CreatedUtcDate).ToList();

            var paged = leaveRequests.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            return new JsonResult(new
            {
                items = paged.Select(x => new {
                    leaveType = x.LeaveType,
                    startDate = x.LeaveStartDate.ToString("dddd, d MMMM yyyy"),

                    endDate = x.endDate.ToString("dddd, d MMMM yyyy"),

                    duration = x.DayAmount,
                    status = x.LeaveStatus
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
        public int PaidLeave { get; set; }
        public int UnpaidLeave { get; set; }
    }

    public class LeaveRequestDto
    {
        public int LeaveId { get; set; }
        public int RequesterId { get; set; }
        public string LeaveDescription { get; set; } = string.Empty;

        public string LeaveStatus { get; set; } = string.Empty;

        public DateTime LeaveStartDate { get; set; }

        public DateTime endDate { get; set; }

        public decimal DayAmount { get; set; }

        public string LeaveType { get; set; } = string.Empty;
        public DateTime CreatedUtcDate { get; set; }
    }


    public class ApiResponse<T>
    {
        public T Content { get; set; }
    }
}
