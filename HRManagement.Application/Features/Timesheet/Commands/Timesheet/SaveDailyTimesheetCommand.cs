using HRManagement.Domain.Interfaces;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Commands.Dto;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using MediatR;

namespace HRManagement.Application.Commands.Timesheet;

/// <summary>
/// Saves (creates or replaces) all timesheet entry rows for a specific day.
/// Used by entry, edit, and bundle-entry flows.
/// </summary>
public class SaveDailyTimesheetCommand(SaveDailyTimesheetRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public SaveDailyTimesheetRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetEntryRepository entryRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<SaveDailyTimesheetCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            SaveDailyTimesheetCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var actionerId = (int)employeeId;
            var dto = command.RequestDto;
            var entryDate = DateOnly.ParseExact(dto.Date, "yyyy-MM-dd");
            var dayType = dto.DayType.ToLower();

            List<TimesheetEntry> entries;

            if (dayType == "working")
            {
                entries = dto.Entries.Select(row => new TimesheetEntry(
                    employeeId: employeeId,
                    entryDate: entryDate,
                    durationMinutes: row.DurationMinutes,
                    projectId: row.ProjectId,
                    applicationUsed: row.ApplicationUsed,
                    taskDescription: row.TaskDescription,
                    projectLeadId: row.ProjectLeadId,
                    location: row.Location,
                    actionerId: actionerId,
                    dayType: "working"
                )).ToList();
            }
            else
            {
                // Create a single placeholder entry for Holiday/Off to preserve the day status in DB
                // Using ProjectId = 1 (Standard) as a placeholder if DayType != working
                entries = new List<TimesheetEntry> { 
                    new TimesheetEntry(
                        employeeId: employeeId,
                        entryDate: entryDate,
                        durationMinutes: 0,
                        projectId: 1, // Placeholder
                        applicationUsed: "SYSTEM",
                        taskDescription: dayType == "holiday" ? "Public Holiday" : "Day Off",
                        projectLeadId: 1, // Placeholder
                        location: 0,
                        actionerId: actionerId,
                        dayType: dayType
                    )
                };
            }
 
            await entryRepository.SaveDailyEntriesAsync(employeeId, entryDate, entries);
 
            return ApiHelperResponse.Success("Timesheet saved successfully.", "Success");
        }
    }
}




