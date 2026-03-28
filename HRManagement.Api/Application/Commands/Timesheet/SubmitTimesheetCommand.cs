using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Commands.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using MediatR;

namespace HRManagement.Api.Application.Commands.Timesheet;

/// <summary>
/// Submits the employee's monthly timesheet for supervisor review.
/// Creates a new submission or re-submits after revision.
/// </summary>
public class SubmitTimesheetCommand(SubmitTimesheetRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public SubmitTimesheetRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetRepository timesheetRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<SubmitTimesheetCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            SubmitTimesheetCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var actionerId = (long)employeeId;
            var dto = command.RequestDto;

            // Note: Basic range and future date validation is now handled by SubmitTimesheetValidator
            // which is automatically executed by the base controller.

            // Check for missing entries
            var missingDays = await timesheetRepository.GetMissingEntryDatesAsync(employeeId, dto.Year, dto.Month);
            if (missingDays.Count > 0)
            {
                var missingDisplay = string.Join(", ", missingDays.Take(5).Select(d => d.ToString("dd MMM yyyy")));
                return ApiHelperResponse.Failed<string>(
                    $"Pengajuan gagal karena terdapat {missingDays.Count} hari kerja tanpa entitas di bulan tersebut: {missingDisplay}. Harap selesaikan sebelum mengirim.");
            }

            var existing = await timesheetRepository.GetSubmissionAsync(employeeId, dto.Year, dto.Month);

            if (existing == null)
            {
                var submission = new TimesheetSubmission(employeeId, dto.Year, dto.Month, actionerId);
                await timesheetRepository.AddSubmissionAsync(submission);
            }
            else
            {
                if (existing.Status == 1)
                {
                    var monthName = new System.Globalization.CultureInfo("en-US").DateTimeFormat.GetMonthName(dto.Month);
                    return ApiHelperResponse.Failed<string>($"Timesheet untuk {monthName} {dto.Year} sudah disetujui oleh Supervisor dan tidak dapat dikirim ulang.");
                }

                existing.Resubmit(actionerId);
                await timesheetRepository.UpdateSubmissionAsync(existing);
            }

            return ApiHelperResponse.Success("Timesheet submitted successfully.", "Success");
        }
    }
}
