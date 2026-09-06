using System;

namespace HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto
{
    public class HolidayDto
    {
        public int Id { get; set; }
        public DateTime HolidayDate { get; set; }
        public string HolidayName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}


