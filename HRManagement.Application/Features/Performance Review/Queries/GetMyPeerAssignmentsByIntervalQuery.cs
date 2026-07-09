using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Features.PerformanceReview.Assignments.Queries;

public record GetMyPeerAssignmentsByIntervalQuery(
    int IntervalId,
    int CurrentEmployeeId
) : IRequest<Result<List<FillAssignmentDetailResponseDto>>>;

internal sealed class GetMyPeerAssignmentsByIntervalQueryHandler(
    IFillAssignmentRepository assignmentRepository,
    ILogger<GetMyPeerAssignmentsByIntervalQueryHandler> logger)
    : IRequestHandler<GetMyPeerAssignmentsByIntervalQuery, Result<List<FillAssignmentDetailResponseDto>>>
{
    public async Task<Result<List<FillAssignmentDetailResponseDto>>> Handle(
        GetMyPeerAssignmentsByIntervalQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName} for Interval ID: {IntervalId} and Employee ID: {EmployeeId}",
            nameof(GetMyPeerAssignmentsByIntervalQueryHandler),
            request.IntervalId,
            request.CurrentEmployeeId);

        var data = await assignmentRepository
            .GetPeerAssignmentDetailsByIntervalAsync(request.CurrentEmployeeId, request.IntervalId, cancellationToken);

        if (data is null)
        {
            return Result.Failure<List<FillAssignmentDetailResponseDto>>(
                "Gagal mengambil data tugas peer review");
        }

        return Result.Success(data);
    }
}