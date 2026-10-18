using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Assignments.Queries;

public record GetSupervisorSelfAssessmentReviewQuery(
    int IntervalId,
    int SubjectId
) : IRequest<Result<FillAssignmentDetailResponseDto>>;

internal sealed class GetSupervisorSelfAssessmentReviewQueryHandler(
    IAssignmentRepository assignmentRepository,
    ILogger<GetSupervisorSelfAssessmentReviewQueryHandler> logger)
    : IRequestHandler<GetSupervisorSelfAssessmentReviewQuery, Result<FillAssignmentDetailResponseDto>>
{
    public async Task<Result<FillAssignmentDetailResponseDto>> Handle(
        GetSupervisorSelfAssessmentReviewQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName} for Interval ID: {IntervalId}, Subject ID: {SubjectId}",
            nameof(GetSupervisorSelfAssessmentReviewQueryHandler),
            request.IntervalId,
            request.SubjectId);

        var data = await assignmentRepository
            .GetSupervisorSelfAssessmentReviewAsync(
                request.SubjectId,
                request.IntervalId,
                cancellationToken);

        if (data is null)
        {
            return Result.Failure<FillAssignmentDetailResponseDto>(
                "Tugas review self-assessment untuk karyawan ini tidak ditemukan");
        }

        return Result.Success(data);
    }
}