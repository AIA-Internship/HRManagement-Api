using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Assignments.Queries;

public record GetMySupervisorAssignmentBySubjectQuery(
    int IntervalId,
    int SupervisorId,
    int SubjectId
) : IRequest<Result<FillAssignmentDetailResponseDto>>;

internal sealed class GetMySupervisorAssignmentBySubjectQueryHandler(
    IAssignmentRepository assignmentRepository,
    ILogger<GetMySupervisorAssignmentBySubjectQueryHandler> logger)
    : IRequestHandler<GetMySupervisorAssignmentBySubjectQuery, Result<FillAssignmentDetailResponseDto>>
{
    public async Task<Result<FillAssignmentDetailResponseDto>> Handle(
        GetMySupervisorAssignmentBySubjectQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName} for Interval ID: {IntervalId}, Supervisor ID: {SupervisorId}, Subject ID: {SubjectId}",
            nameof(GetMySupervisorAssignmentBySubjectQueryHandler),
            request.IntervalId,
            request.SupervisorId,
            request.SubjectId);

        var data = await assignmentRepository
            .GetSupervisorAssignmentBySubjectAsync(
                request.SupervisorId,
                request.SubjectId,
                request.IntervalId,
                cancellationToken);

        if (data is null)
        {
            return Result.Failure<FillAssignmentDetailResponseDto>(
                "Tugas supervisor review untuk karyawan ini tidak ditemukan");
        }

        return Result.Success(data);
    }
}