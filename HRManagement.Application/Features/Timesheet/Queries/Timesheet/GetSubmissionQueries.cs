using HRManagement.Domain.Interfaces;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Application.Queries.Timesheet;

// ── Active Submission Status ──────────────────────────────────────────────────

/// <summary>
/// Returns the current active submission status for a given month.
/// </summary>
public class GetSubmissionStatusQuery(int year, int month)
    : IRequest<ApiResponse<SubmissionStatusDto>>
{
    public int Year { get; } = year;
    public int Month { get; } = month;

    public class Handler(
        ITimesheetSubmissionRepository submissionRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetSubmissionStatusQuery, ApiResponse<SubmissionStatusDto>>
    {
        public async Task<ApiResponse<SubmissionStatusDto>> Handle(
            GetSubmissionStatusQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var submission = await submissionRepository.GetSubmissionAsync(employeeId, request.Year, request.Month);
 
            var result = submission == null
                ? new SubmissionStatusDto { Year = request.Year, Month = request.Month, Status = "Not Submitted" }
                : new SubmissionStatusDto
                {
                    SubmissionId = submission.Id,
                    Year = submission.Year,
                    Month = submission.Month,
                    Status = MapStatus(submission.Status),
                    SubmittedDate = submission.SubmittedDate.ToString("yyyy-MM-dd HH:mm"),
                    ReviewedDate = submission.ReviewedDate?.ToString("yyyy-MM-dd HH:mm"),
                    RevisionNote = submission.RevisionNote
                };
 
            return ApiHelperResponse.Success("Submission status retrieved successfully.", result);
        }
 
        private static string MapStatus(int status) => status switch
        {
            0 => "Waiting for Approval",
            1 => "Approved",
            2 => "Need Revision",
            _ => "Not Submitted"
        };
    }
}
 
// ── Submission History ────────────────────────────────────────────────────────
 
/// <summary>
/// Returns the full submission history for the logged-in employee.
/// </summary>
public class GetSubmissionHistoryQuery : IRequest<ApiResponse<List<SubmissionHistoryItemDto>>>
{
    public class Handler(
        ITimesheetSubmissionRepository submissionRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetSubmissionHistoryQuery, ApiResponse<List<SubmissionHistoryItemDto>>>
    {
        public async Task<ApiResponse<List<SubmissionHistoryItemDto>>> Handle(
            GetSubmissionHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var submissions = await submissionRepository.GetSubmissionsByEmployeeAsync(employeeId);

            var result = submissions.Select(s => new SubmissionHistoryItemDto
            {
                SubmissionId = s.Id,
                Year = s.Year,
                Month = s.Month,
                SubmittedDate = s.SubmittedDate.ToString("yyyy-MM-dd HH:mm"),
                Status = MapStatus(s.Status),
                RevisionNote = s.RevisionNote
            }).ToList();

            return ApiHelperResponse.Success("Submission history retrieved successfully.", result);
        }

        private static string MapStatus(int status) => status switch
        {
            0 => "Waiting for Approval",
            1 => "Approved",
            2 => "Need Revision",
            _ => "Unknown"
        };
    }
}




