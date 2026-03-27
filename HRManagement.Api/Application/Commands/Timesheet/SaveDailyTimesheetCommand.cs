using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Commands.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using MediatR;

namespace HRManagement.Api.Application.Commands.Timesheet;

/// <summary>
/// Saves (creates or replaces) all timesheet entry rows for a specific day.
/// Used by entry, edit, and bundle-entry flows.
/// </summary>
public class SaveDailyTimesheetCommand(SaveDailyTimesheetRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public SaveDailyTimesheetRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetRepository timesheetRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<SaveDailyTimesheetCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            SaveDailyTimesheetCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var actionerId = (long)employeeId;
            var dto = command.RequestDto;

            if (!DateOnly.TryParseExact(dto.Date, "yyyy-MM-dd", out var entryDate))
            {
                return ApiHelperResponse.Failed<string>($"Format tanggal '{dto.Date}' tidak valid. Gunakan format yyyy-MM-dd.");
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));

            // Business rule: interns can only log entries for today or past dates in the same month
            if (entryDate > today)
            {
                return ApiHelperResponse.Failed<string>($"Anda tidak dapat mengisi timesheet untuk tanggal masa depan ({entryDate:dd MMM yyyy}).");
            }

            var entries = dto.Entries.Select(row => new TimesheetEntry(
                employeeId: employeeId,
                entryDate: entryDate,
                durationMinutes: row.DurationMinutes,
                projectId: row.ProjectId,
                applicationUsed: row.ApplicationUsed,
                taskDescription: row.TaskDescription,
                projectLeadId: row.ProjectLeadId,
                location: row.Location,
                actionerId: actionerId
            )).ToList();

            await timesheetRepository.SaveDailyEntriesAsync(employeeId, entryDate, entries);

            return ApiHelperResponse.Success("Timesheet saved successfully.", "Success");
        }
    }
}
