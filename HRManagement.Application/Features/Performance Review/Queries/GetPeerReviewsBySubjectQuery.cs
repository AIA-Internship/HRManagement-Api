using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Assignments.Queries;

public record GetPeerReviewsBySubjectQuery(
    int SupervisorId,
    int IntervalId,
    int SubjectId
) : IRequest<Result<PeerReviewMatrixResponseDto>>;

internal sealed class GetPeerReviewsBySubjectQueryHandler(
    IAssignmentRepository assignmentRepository,
    ILogger<GetPeerReviewsBySubjectQueryHandler> logger)
    : IRequestHandler<GetPeerReviewsBySubjectQuery, Result<PeerReviewMatrixResponseDto>>
{
    public async Task<Result<PeerReviewMatrixResponseDto>> Handle(
        GetPeerReviewsBySubjectQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName} for Interval ID: {IntervalId}, Subject ID: {SubjectId}",
            nameof(GetPeerReviewsBySubjectQueryHandler),
            request.IntervalId,
            request.SubjectId);

        var data = await assignmentRepository
            .GetPeerReviewsBySubjectAsync(
                request.SupervisorId,
                request.IntervalId,
                request.SubjectId,
                cancellationToken);

        if (data is null)
        {
            return Result.Failure<PeerReviewMatrixResponseDto>(
                "Data peer review untuk karyawan ini tidak ditemukan.");
        }

        return Result.Success(data);
    }
}