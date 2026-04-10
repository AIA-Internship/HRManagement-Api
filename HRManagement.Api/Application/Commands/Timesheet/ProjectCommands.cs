using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Commands.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using MediatR;

namespace HRManagement.Api.Application.Commands.Timesheet;

/// <summary>
/// Creates a new project (supervisor only).
/// </summary>
public class CreateProjectCommand(CreateProjectRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public CreateProjectRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetRepository timesheetRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<CreateProjectCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            CreateProjectCommand command,
            CancellationToken cancellationToken)
        {
            var actionerId = (long)currentUserService.UserId;
            var dto = command.RequestDto;

            var project = new TimesheetProject(dto.Name, dto.Description, actionerId);
            await timesheetRepository.AddProjectAsync(project);

            return ApiHelperResponse.Success("Project created successfully.", "Success");
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Updates an existing project (supervisor only).
/// </summary>
public class UpdateProjectCommand(int projectId, UpdateProjectRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public int ProjectId { get; } = projectId;
    public UpdateProjectRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetRepository timesheetRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<UpdateProjectCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            UpdateProjectCommand command,
            CancellationToken cancellationToken)
        {
            var actionerId = (long)currentUserService.UserId;
            var dto = command.RequestDto;

            var project = await timesheetRepository.GetProjectByIdAsync(command.ProjectId);
            if (project == null)
            {
                return ApiHelperResponse.Failed<string>("Project not found.");
            }

            project.UpdateDetails(dto.Name, dto.Description, dto.Status, actionerId);
            await timesheetRepository.UpdateProjectAsync(project);

            return ApiHelperResponse.Success("Project updated successfully.", "Success");
        }
    }
}
