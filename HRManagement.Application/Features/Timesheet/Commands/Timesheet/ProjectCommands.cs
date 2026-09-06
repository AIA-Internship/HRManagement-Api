using HRManagement.Domain.Interfaces;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Commands.Dto;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Application.Commands.Timesheet;

/// <summary>
/// Creates a new project (supervisor only).
/// </summary>
public class CreateProjectCommand(CreateProjectRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public CreateProjectRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetProjectRepository projectRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<CreateProjectCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            CreateProjectCommand command,
            CancellationToken cancellationToken)
        {
            var actionerId = (int)currentUserService.UserId;
            var dto = command.RequestDto;

            var project = new TimesheetProject(dto.Name, dto.Description, dto.ProjectLeader, actionerId);
            await projectRepository.AddAsync(project);

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
        ITimesheetProjectRepository projectRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<UpdateProjectCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            UpdateProjectCommand command,
            CancellationToken cancellationToken)
        {
            var actionerId = (int)currentUserService.UserId;
            var dto = command.RequestDto;

            var project = await projectRepository.GetByIdAsync(command.ProjectId);
            if (project == null)
            {
                return ApiHelperResponse.Failed<string>("Project not found.");
            }

            project.UpdateDetails(dto.Name, dto.Description, dto.ProjectLeader, dto.Status, actionerId);
            await projectRepository.UpdateAsync(project);

            return ApiHelperResponse.Success("Project updated successfully.", "Success");
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Bulk-upserts the entire project list from the Edit Project page.
/// Items with Id = null are created; items with Id > 0 are updated.
/// Projects that exist in the DB but are absent from the payload are soft-deleted.
/// </summary>
public class BulkUpsertProjectsCommand(BulkUpsertProjectsRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public BulkUpsertProjectsRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetProjectRepository projectRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<BulkUpsertProjectsCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            BulkUpsertProjectsCommand command,
            CancellationToken cancellationToken)
        {
            var actionerId = (int)currentUserService.UserId;
            var dto = command.RequestDto;

            if (dto.Projects == null || dto.Projects.Count == 0)
            {
                return ApiHelperResponse.Failed<string>("Project list cannot be empty.");
            }

            // Validate: all rows must have Name and ProjectLeader
            var invalid = dto.Projects.Any(p =>
                string.IsNullOrWhiteSpace(p.ProjectName) ||
                string.IsNullOrWhiteSpace(p.ProjectLeader));

            if (invalid)
            {
                return ApiHelperResponse.Failed<string>("All projects must have a Name and Project Leader.");
            }

            // Fetch all existing non-deleted projects
            var existingProjects = await projectRepository.GetActiveListAsync();

            // IDs submitted in the payload
            var submittedIds = dto.Projects
                .Where(p => p.Id.HasValue && p.Id > 0)
                .Select(p => p.Id!.Value)
                .ToHashSet();

            // Soft-delete projects that are no longer in the list
            foreach (var existing in existingProjects.Where(p => !submittedIds.Contains(p.Id)))
            {
                await projectRepository.DeleteAsync(existing);
            }

            // Upsert each row
            foreach (var item in dto.Projects)
            {
                if (item.Id.HasValue && item.Id > 0)
                {
                    // Update existing
                    var project = existingProjects.FirstOrDefault(p => p.Id == item.Id);
                    if (project != null)
                    {
                        project.UpdateDetails(item.ProjectName, item.Description, item.ProjectLeader, 0, actionerId);
                        await projectRepository.UpdateAsync(project);
                    }
                }
                else
                {
                    // Create new
                    var newProject = new TimesheetProject(item.ProjectName, item.Description, item.ProjectLeader, actionerId);
                    try
                    {
                        await projectRepository.AddAsync(newProject);
                    }
                    catch (DbUpdateException)
                    {
                        return ApiHelperResponse.Failed<string>($"Project name '{item.ProjectName}' already exists.");
                    }
                }
            }

            await projectRepository.SaveChangesAsync();

            return ApiHelperResponse.Success("Projects updated successfully.", "Success");
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Soft-deletes a project by ID (supervisor only).
/// </summary>
public class DeleteProjectCommand(int projectId)
    : IRequest<ApiResponse<string>>
{
    public int ProjectId { get; } = projectId;

    public class Handler(
        ITimesheetProjectRepository projectRepository)
        : IRequestHandler<DeleteProjectCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            DeleteProjectCommand command,
            CancellationToken cancellationToken)
        {
            var project = await projectRepository.GetByIdAsync(command.ProjectId);
            if (project == null)
            {
                return ApiHelperResponse.Failed<string>("Project not found.");
            }

            await projectRepository.DeleteAsync(project);
            return ApiHelperResponse.Success("Project deleted successfully.", "Success");
        }
    }
}




