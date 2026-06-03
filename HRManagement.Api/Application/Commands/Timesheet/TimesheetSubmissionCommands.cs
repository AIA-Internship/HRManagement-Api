using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Commands.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Commands.Timesheet;

/// <summary>
/// Submits the employee's monthly timesheet for supervisor review.
/// </summary>
public class SubmitTimesheetCommand(SubmitTimesheetRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public SubmitTimesheetRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetEntryRepository entryRepository,
        ITimesheetSubmissionRepository submissionRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<SubmitTimesheetCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(SubmitTimesheetCommand command, CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var actionerId = (long)employeeId;
            var dto = command.RequestDto;

            var missingDays = await entryRepository.GetMissingEntryDatesAsync(employeeId, dto.Year, dto.Month);
            if (missingDays.Count > 0)
            {
                var missingDisplay = string.Join(", ", missingDays.Take(5).Select(d => d.ToString("dd MMM yyyy")));
                return ApiHelperResponse.Failed<string>($"Pengajuan gagal karena terdapat {missingDays.Count} hari kerja tanpa entitas: {missingDisplay}.");
            }
 
            var existing = await submissionRepository.GetSubmissionAsync(employeeId, dto.Year, dto.Month);
            if (existing == null) await submissionRepository.AddAsync(new TimesheetSubmission(employeeId, dto.Year, dto.Month, actionerId));
            else {
                if (existing.Status == 1) return ApiHelperResponse.Failed<string>("Timesheet sudah disetujui dan tidak dapat dikirim ulang.");
                existing.Resubmit(actionerId);
                await submissionRepository.UpdateAsync(existing);
            }

            return ApiHelperResponse.Success("Timesheet submitted successfully.", "Success");
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Supervisor approves an intern's submitted monthly timesheet.
/// </summary>
public class ApproveTimesheetCommand(ApproveTimesheetRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public ApproveTimesheetRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetSubmissionRepository submissionRepository,
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService)
        : IRequestHandler<ApproveTimesheetCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(ApproveTimesheetCommand command, CancellationToken cancellationToken)
        {
            var supervisorId = currentUserService.UserId;
            var supervisor = await appDbContext.Employees.FindAsync(new object[] { supervisorId }, cancellationToken);
            var dto = command.RequestDto;

            var submission = await submissionRepository.GetByIdAsync(dto.SubmissionId);
            if (submission == null) return ApiHelperResponse.Failed<string>("Submission not found.");

            // Security Check
            var intern = await appDbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == submission.EmployeeId, cancellationToken);
            if (intern != null) intern.EmploymentInformation = await appDbContext.EmploymentInformation.AsNoTracking().FirstOrDefaultAsync(ei => ei.EmployeeId == intern.Id, cancellationToken);
            
            if (intern?.EmploymentInformation?.SupervisorName != supervisor?.FullName) return ApiHelperResponse.Failed<string>("Access Denied: You are not authorized.");
            if (submission.Status != 0) return ApiHelperResponse.Failed<string>("Only 'Waiting for Approval' can be approved.");

            submission.Approve(supervisorId, (long)supervisorId);
            await submissionRepository.UpdateAsync(submission);

            return ApiHelperResponse.Success("Timesheet approved successfully.", "Approved");
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Supervisor gives revision feedback on an intern's submitted timesheet.
/// </summary>
public class GiveRevisionCommand(GiveRevisionRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public GiveRevisionRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetSubmissionRepository submissionRepository,
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService)
        : IRequestHandler<GiveRevisionCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(GiveRevisionCommand command, CancellationToken cancellationToken)
        {
            var supervisorId = currentUserService.UserId;
            var supervisor = await appDbContext.Employees.FindAsync(new object[] { supervisorId }, cancellationToken);
            var dto = command.RequestDto;

            var submission = await submissionRepository.GetByIdAsync(dto.SubmissionId);
            if (submission == null) return ApiHelperResponse.Failed<string>("Submission not found.");

            // Security Check
            var intern = await appDbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == submission.EmployeeId, cancellationToken);
            if (intern != null) intern.EmploymentInformation = await appDbContext.EmploymentInformation.AsNoTracking().FirstOrDefaultAsync(ei => ei.EmployeeId == intern.Id, cancellationToken);

            if (intern?.EmploymentInformation?.SupervisorName != supervisor?.FullName) return ApiHelperResponse.Failed<string>("Access Denied.");
            if (submission.Status != 0) return ApiHelperResponse.Failed<string>("Revision can only be given to 'Waiting for Approval' status.");

            submission.GiveRevision(supervisorId, dto.OverallNote, (long)supervisorId);
            await submissionRepository.UpdateAsync(submission);

            if (dto.DayComments.Any())
            {
                var dayComments = dto.DayComments
                    .Where(dc => DateOnly.TryParseExact(dc.Date, "yyyy-MM-dd", out _))
                    .Select(dc => {
                        DateOnly.TryParseExact(dc.Date, "yyyy-MM-dd", out var parsedDate);
                        return new TimesheetDayComment(submission.Id, parsedDate, dc.Comment, (long)supervisorId);
                    }).ToList();

                await submissionRepository.SaveDayCommentsAsync(submission.Id, dayComments);
            }

            return ApiHelperResponse.Success("Revision note sent successfully.", "Need Revision");
        }
    }
}
