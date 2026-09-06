using System;
using HRManagement.Domain.Models.Tables;

namespace HRManagement.Domain.Models.Tables
{
    /// <summary>
    /// Represents a public holiday or company holiday.
    /// Used to prevent timesheet entries on holidays or mark them specifically.
    /// </summary>
    public class TimesheetHoliday : BaseTable
    {
        public int Id { get; private set; }
        public DateTime HolidayDate { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }

        protected TimesheetHoliday() { }

        public TimesheetHoliday(DateTime holidayDate, string name, string? description, int actionerId)
        {
            HolidayDate = holidayDate.Date;
            Name = name;
            Description = description;

            MarkAsCreated(actionerId);
            MarkAsModified(actionerId);
        }

        public void UpdateDetails(DateTime holidayDate, string name, string? description, int actionerId)
        {
            HolidayDate = holidayDate.Date;
            Name = name;
            Description = description;

            MarkAsModified(actionerId);
        }
    }
}



