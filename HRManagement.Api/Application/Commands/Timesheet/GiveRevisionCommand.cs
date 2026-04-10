using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Commands.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Commands.Timesheet;

/// <summary>
/// Supervisor gives revision feedback on an intern's submitted timesheet.
/// </summary>
public class GiveRevisionCommand(GiveRevisionRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public GiveRevisionRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetRepository timesheetRepository,
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService)
        : IRequestHandler<GiveRevisionCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            GiveRevisionCommand command,
            CancellationToken cancellationToken)
        {
            var supervisorId = currentUserService.UserId;
            var supervisor = await appDbContext.Employees.FindAsync(new object[] { supervisorId }, cancellationToken);
            var dto = command.RequestDto;

            var submission = await timesheetRepository.GetSubmissionByIdAsync(dto.SubmissionId);

            if (submission == null)
            {
                return ApiHelperResponse.Failed<string>("Submission not found.");
            }

            // Security Check
            var intern = await appDbContext.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == submission.EmployeeId, cancellationToken);

            if (intern != null)
            {
                intern.EmploymentInformation = await appDbContext.EmploymentInformation
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ei => ei.EmployeeId == intern.Id, cancellationToken);
            }

            if (intern?.EmploymentInformation?.SupervisorName != supervisor?.FullName)
            {
                return ApiHelperResponse.Failed<string>("Access Denied: You are not authorized to give revision to this submission.");
            }

            if (submission.Status != 0)
            {
                return ApiHelperResponse.Failed<string>(
                    "Revision can only be given to submissions with 'Waiting for Approval' status.");
            }

            submission.GiveRevision(supervisorId, dto.OverallNote, (long)supervisorId);
            await timesheetRepository.UpdateSubmissionAsync(submission);

            // Save per-day comments
            if (dto.DayComments.Any())
            {
                var dayComments = dto.DayComments
                    .Where(dc => DateOnly.TryParseExact(dc.Date, "yyyy-MM-dd", out _))
                    .Select(dc =>
                    {
                        DateOnly.TryParseExact(dc.Date, "yyyy-MM-dd", out var parsedDate);
                        return new TimesheetDayComment(
                            submission.Id,
                            parsedDate,
                            dc.Comment,
                            (long)supervisorId);
                    }).ToList();

                await timesheetRepository.SaveDayCommentsAsync(submission.Id, dayComments);
            }

            return ApiHelperResponse.Success("Revision note sent successfully.", "Need Revision");
        }
    }
}
