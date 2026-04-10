using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Commands.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Commands.Timesheet;

/// <summary>
/// Supervisor approves an intern's submitted monthly timesheet.
/// </summary>
public class ApproveTimesheetCommand(ApproveTimesheetRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public ApproveTimesheetRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetRepository timesheetRepository,
        IApplicationDbContext appDbContext,
        ICurrentUserService currentUserService)
        : IRequestHandler<ApproveTimesheetCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            ApproveTimesheetCommand command,
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
                return ApiHelperResponse.Failed<string>("Access Denied: You are not authorized to approve this submission.");
            }

            if (submission.Status != 0)
            {
                return ApiHelperResponse.Failed<string>(
                    "Only submissions with 'Waiting for Approval' status can be approved.");
            }

            submission.Approve(supervisorId, (long)supervisorId);
            await timesheetRepository.UpdateSubmissionAsync(submission);

            return ApiHelperResponse.Success("Timesheet approved successfully.", "Approved");
        }
    }
}
