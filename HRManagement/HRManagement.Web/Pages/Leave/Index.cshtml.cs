using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Threading.Tasks;

namespace HRManagement.Web.Pages.Leave
{

    public class CalendarEvent
    {
        // REQUIRED
        public string id { get; set; } = default!;
        public string title { get; set; } = default!;
        public DateTime start { get; set; }

        // OPTIONAL
        public DateTime? end { get; set; }
        public bool? allDay { get; set; }

        public string? groupId { get; set; }

        public string? startStr { get; set; }
        public string? endStr { get; set; }

        public string? url { get; set; }

        public List<string>? classNames { get; set; }

        public bool? editable { get; set; }
        public bool? startEditable { get; set; }
        public bool? durationEditable { get; set; }
        public bool? resourceEditable { get; set; }

        public string? display { get; set; }
        public bool? overlap { get; set; }

        public object? constraint { get; set; }

        public string? backgroundColor { get; set; }
        public string? borderColor { get; set; }
        public string? textColor { get; set; }

        public Dictionary<string, object>? extendedProps { get; set; }

        public object? source { get; set; }
    }

    public class ReadLeaveRequestDto
    {
        public int? leaveId { get; set; }
        public int? requesterId { get; set; }
        public int? supervisorId { get; set; }
        public string? leaveDescription { get; set; }
        public string? leaveStatus { get; set; }
        public DateTime? leaveStartDate { get; set; }
        public int? dayAmount { get; set; }
        public string? leaveType { get; set; }
        public bool? isCompleted { get; set; }
        public bool? isEdit { get; set; }
        public int? initialRequestId { get; set; }
        public string[]? attachmentPath { get; set; }
        public DateTime createdUtcDate { get; set; }
    }
    public class IndexModel : PageModel
    {
        public string EventJson { get; set; } = "";
        public string LeaveRequestsJson { get; set; } = "";

        public List<CalendarEvent> calendarEvents = new List<CalendarEvent>();
        public List<ReadLeaveRequestDto> leaveRequests = new List<ReadLeaveRequestDto>();
        public async Task OnGet(int? month, int? year)
        {

            
                int selectedMonth = month ?? DateTime.Now.Month;
                int selectedYear = year ?? DateTime.Now.Year;
                //dummy data
                calendarEvents.Add(
                new CalendarEvent
                {
                    id = "1",
                    title = "Paid Leave",
                    start = new DateTime(2026, 3, 18),
                    allDay = true
                }
            );
            calendarEvents.Add(new CalendarEvent
            {
                id = "2",
                title = "Unpaid Leave",
                start = new DateTime(2026, 3, 20),
                end = new DateTime(2026, 3, 22),
                allDay = true
            });

            leaveRequests.Add(new ReadLeaveRequestDto
            {
                leaveId = 1,
                requesterId = 1,
                supervisorId = 2,
                leaveDescription = "Going to the beach",
                leaveStatus = "Approved",
                leaveStartDate = new DateTime(2026, 3, 20),
                dayAmount = 3,
                leaveType = "Unpaid Leave",
                isCompleted = false,
                isEdit = false,
                initialRequestId = null,
                attachmentPath = null,
                createdUtcDate = DateTime.UtcNow
            });

            //==================================

            //get data from database
            //await loadData(month, year);


            EventJson = JsonSerializer.Serialize(calendarEvents);
            LeaveRequestsJson = JsonSerializer.Serialize(leaveRequests);

            if(!month.HasValue && !year.HasValue)
            {
                ViewData["SelectedDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewData["SelectedDate"] = new DateTime(selectedYear, selectedMonth, 1)
                .ToString("yyyy-MM-dd");
            }


        }

        public async Task loadData(int month, int year)
        {
            var appUrl = Environment.GetEnvironmentVariable("APP_URL") ?? "https://localhost:7060";
            using var client = new HttpClient();

            var response = await client.GetAsync(
                $"{appUrl}/api/leave/get-by-month?month={month}&year={year}"
            );

            if (!response.IsSuccessStatusCode) return;

            var content = await response.Content.ReadFromJsonAsync<List<ReadLeaveRequestDto>>();
            if (content == null) return;

            calendarEvents.Clear();

            foreach (var item in content)
            {
                calendarEvents.Add(new CalendarEvent
                {
                    id = item.leaveId?.ToString(),
                    title = item.leaveType,
                    start = item.leaveStartDate ?? DateTime.MinValue,
                    end = item.leaveStartDate.HasValue && item.dayAmount.HasValue
                        ? item.leaveStartDate.Value.AddDays(item.dayAmount.Value)
                        : (DateTime?)null,
                    allDay = true
                });
            }

            leaveRequests = content;
        }
    }
}
