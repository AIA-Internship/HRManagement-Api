using System;
using System.Collections.Generic;

namespace HRManagement.Domain.Models.Payload.TimesheetDtos.Commands.Dto
{
    public class HolidayUpsertDto
    {
        public int? Id { get; set; }
        public DateTime HolidayDate { get; set; }
        public string HolidayName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class BulkUpsertHolidaysDto
    {
        public List<HolidayUpsertDto> Holidays { get; set; } = new();
    }
}


