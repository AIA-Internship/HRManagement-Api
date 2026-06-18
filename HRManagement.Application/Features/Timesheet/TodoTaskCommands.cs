using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Commands.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using MediatR;

namespace HRManagement.Api.Application.Commands.Timesheet;

/// <summary>
/// Creates a new to-do task on the employee's dashboard.
/// </summary>
public class CreateTodoTaskCommand(CreateTodoTaskRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public CreateTodoTaskRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetRepository timesheetRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<CreateTodoTaskCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            CreateTodoTaskCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var actionerId = (long)employeeId;
            var dto = command.RequestDto;

            DateOnly? dueDate = null;
            if (!string.IsNullOrWhiteSpace(dto.DueDate) &&
                DateOnly.TryParseExact(dto.DueDate, "yyyy-MM-dd", out var parsed))
            {
                dueDate = parsed;
            }

            var task = new TodoTask(employeeId, dto.TaskName, dueDate, dto.Priority, actionerId);
            await timesheetRepository.AddTodoTaskAsync(task);

            return ApiHelperResponse.Success("To-do task created successfully.", "Success");
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Updates a to-do task's details.
/// </summary>
public class UpdateTodoTaskCommand(int taskId, UpdateTodoTaskRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public int TaskId { get; } = taskId;
    public UpdateTodoTaskRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITimesheetRepository timesheetRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<UpdateTodoTaskCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            UpdateTodoTaskCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var dto = command.RequestDto;

            var task = await timesheetRepository.GetTodoTaskByIdAsync(command.TaskId);
            if (task == null)
            {
                return ApiHelperResponse.Failed<string>("Task not found.");
            }

            if (task.EmployeeId != employeeId)
            {
                return ApiHelperResponse.Failed<string>("You are not authorized to update this task.");
            }

            DateOnly? dueDate = null;
            if (!string.IsNullOrWhiteSpace(dto.DueDate) &&
                DateOnly.TryParseExact(dto.DueDate, "yyyy-MM-dd", out var parsed))
            {
                dueDate = parsed;
            }

            task.UpdateDetails(dto.TaskName, dueDate, dto.Priority, (long)employeeId);
            await timesheetRepository.UpdateTodoTaskAsync(task);

            return ApiHelperResponse.Success("Task updated successfully.", "Success");
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Toggles the completed status of a to-do task.
/// </summary>
public class ToggleTodoTaskCommand(int taskId) : IRequest<ApiResponse<string>>
{
    public int TaskId { get; } = taskId;

    public class Handler(
        ITimesheetRepository timesheetRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<ToggleTodoTaskCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            ToggleTodoTaskCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;

            var task = await timesheetRepository.GetTodoTaskByIdAsync(command.TaskId);
            if (task == null)
            {
                return ApiHelperResponse.Failed<string>("Task not found.");
            }

            if (task.EmployeeId != employeeId)
            {
                return ApiHelperResponse.Failed<string>("You are not authorized to update this task.");
            }

            task.ToggleCompleted((long)employeeId);
            await timesheetRepository.UpdateTodoTaskAsync(task);

            return ApiHelperResponse.Success("Task status updated.", "Success");
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Soft-deletes a to-do task.
/// </summary>
public class DeleteTodoTaskCommand(int taskId) : IRequest<ApiResponse<string>>
{
    public int TaskId { get; } = taskId;

    public class Handler(
        ITimesheetRepository timesheetRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<DeleteTodoTaskCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            DeleteTodoTaskCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;

            var task = await timesheetRepository.GetTodoTaskByIdAsync(command.TaskId);
            if (task == null)
            {
                return ApiHelperResponse.Failed<string>("Task not found.");
            }

            if (task.EmployeeId != employeeId)
            {
                return ApiHelperResponse.Failed<string>("You are not authorized to delete this task.");
            }

            await timesheetRepository.DeleteTodoTaskAsync(task);

            return ApiHelperResponse.Success("Task deleted successfully.", "Success");
        }
    }
}
