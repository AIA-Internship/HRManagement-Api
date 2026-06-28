using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Features.PerformanceReview.Assignments.Queries;

public record GetAssignmentDetailByIdQuery(
    int AssignmentId,
    int CurrentEmployeeId
) : IRequest<Result<FillAssignmentDetailResponseDto>>;

internal sealed class GetAssignmentDetailByIdQueryHandler(
    IFillAssignmentRepository assignmentRepository,
    ILogger<GetAssignmentDetailByIdQueryHandler> logger)
    : IRequestHandler<GetAssignmentDetailByIdQuery, Result<FillAssignmentDetailResponseDto>>
{
    public async Task<Result<FillAssignmentDetailResponseDto>> Handle(
        GetAssignmentDetailByIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName} for Assignment ID: {AssignmentId}",
            nameof(GetAssignmentDetailByIdQueryHandler),
            request.AssignmentId);

        var data = await assignmentRepository
            .GetAssignmentDetailByIdAsync(request.AssignmentId, cancellationToken);

        if (data is null)
        {
            return Result.Failure<FillAssignmentDetailResponseDto>(
                "Tugas penilaian tidak ditemukan.");
        }

        if (request.CurrentEmployeeId != data.FillerId && request.CurrentEmployeeId != data.SubjectId)
        {
            logger.LogWarning(
                "Unauthorized access attempt by User ID {UserId} on Assignment ID {AssignmentId}",
                request.CurrentEmployeeId,
                request.AssignmentId);

            return Result.Failure<FillAssignmentDetailResponseDto>(
                "Anda tidak memiliki akses untuk melihat tugas penilaian ini.");
        }

        return Result.Success(data);
    }
}