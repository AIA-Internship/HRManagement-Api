using HRManagement.Domain.Interfaces;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Commands.Dto;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Application.Commands.Timesheet;

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
            var actionerId = (int)employeeId;
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
            var supervisor = await appDbContext.Employee.FindAsync(new object[] { supervisorId }, cancellationToken);
            var dto = command.RequestDto;

            var submission = await submissionRepository.GetByIdAsync(dto.SubmissionId);
            if (submission == null) return ApiHelperResponse.Failed<string>("Submission not found.");

            // Security Check
            var intern = await appDbContext.Employee
                .Include(e => e.EmploymentInformation)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == submission.EmployeeId, cancellationToken);
            if (intern?.EmploymentInformation?.SupervisorName != supervisor?.FullName) return ApiHelperResponse.Failed<string>("Access Denied: You are not authorized.");
            if (submission.Status != 0) return ApiHelperResponse.Failed<string>("Only 'Waiting for Approval' can be approved.");

            submission.Approve(supervisorId, (int)supervisorId);
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
            var supervisor = await appDbContext.Employee.FindAsync(new object[] { supervisorId }, cancellationToken);
            var dto = command.RequestDto;

            var submission = await submissionRepository.GetByIdAsync(dto.SubmissionId);
            if (submission == null) return ApiHelperResponse.Failed<string>("Submission not found.");

            // Security Check
            var intern = await appDbContext.Employee
                .Include(e => e.EmploymentInformation)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == submission.EmployeeId, cancellationToken);
            if (intern?.EmploymentInformation?.SupervisorName != supervisor?.FullName) return ApiHelperResponse.Failed<string>("Access Denied.");
            if (submission.Status != 0) return ApiHelperResponse.Failed<string>("Revision can only be given to 'Waiting for Approval' status.");

            submission.GiveRevision(supervisorId, dto.OverallNote, (int)supervisorId);
            await submissionRepository.UpdateAsync(submission);

            if (dto.DayComments.Any())
            {
                var dayComments = dto.DayComments
                    .Where(dc => DateOnly.TryParseExact(dc.Date, "yyyy-MM-dd", out _))
                    .Select(dc => {
                        DateOnly.TryParseExact(dc.Date, "yyyy-MM-dd", out var parsedDate);
                        return new TimesheetDayComment(submission.Id, parsedDate, dc.Comment, (int)supervisorId);
                    }).ToList();

                await submissionRepository.SaveDayCommentsAsync(submission.Id, dayComments);
            }

            return ApiHelperResponse.Success("Revision note sent successfully.", "Need Revision");
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Supervisor submits the review: evaluates day-level status and saves it.
/// If all days are [APPROVED], marks submission as Approved.
/// If any day has a revision remark, marks submission as Need Revision.
/// </summary>
public class SubmitSupervisorReviewCommand(SubmitSupervisorReviewRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public SubmitSupervisorReviewRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetSubmissionRepository submissionRepository,
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService)
        : IRequestHandler<SubmitSupervisorReviewCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(SubmitSupervisorReviewCommand command, CancellationToken cancellationToken)
        {
            var supervisorId = currentUserService.UserId;
            var supervisor = await appDbContext.Employee.FindAsync(new object[] { supervisorId }, cancellationToken);
            var dto = command.RequestDto;

            var submission = await submissionRepository.GetByIdAsync(dto.SubmissionId);
            
            // Create a virtual submission if none exists but we are reviewing "anytime"
            if (submission == null && dto.SubmissionId == 0)
            {
                submission = new TimesheetSubmission(dto.EmployeeId, dto.Year, dto.Month, (int)supervisorId);
                await submissionRepository.AddAsync(submission);
                await appDbContext.SaveChangesAsync(cancellationToken);
            }
            
            if (submission == null) return ApiHelperResponse.Failed<string>("Submission not found.");

            var intern = await appDbContext.Employee
                .Include(e => e.EmploymentInformation)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == submission.EmployeeId, cancellationToken);
            if (intern?.EmploymentInformation?.SupervisorName != supervisor?.FullName) return ApiHelperResponse.Failed<string>("Access Denied.");

            // Evaluate comments
            bool needsRevision = dto.ReviewedDays.Any(d => d.Comment != "[APPROVED]");

            if (needsRevision)
            {
                submission.GiveRevision(supervisorId, "See daily remarks for details.", (int)supervisorId);
            }
            else
            {
                submission.Approve(supervisorId, (int)supervisorId);
            }

            await submissionRepository.UpdateAsync(submission);
            await appDbContext.SaveChangesAsync(cancellationToken);

            if (dto.ReviewedDays.Any())
            {
                var dayComments = dto.ReviewedDays
                    .Where(dc => DateOnly.TryParseExact(dc.Date, "yyyy-MM-dd", out _))
                    .Select(dc => {
                        DateOnly.TryParseExact(dc.Date, "yyyy-MM-dd", out var parsedDate);
                        return new TimesheetDayComment(submission.Id, parsedDate, dc.Comment, (int)supervisorId);
                    }).ToList();

                await submissionRepository.SaveDayCommentsAsync(submission.Id, dayComments);
            }

            return ApiHelperResponse.Success(needsRevision ? "Revision requested." : "Timesheet approved.", "Success");
        }
    }
}







