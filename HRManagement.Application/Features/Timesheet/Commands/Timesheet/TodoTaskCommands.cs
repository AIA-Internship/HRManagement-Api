using HRManagement.Domain.Interfaces;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Commands.Dto;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;
using MediatR;

namespace HRManagement.Application.Commands.Timesheet;

/// <summary>
/// Creates a new to-do task on the employee's dashboard.
/// </summary>
public class CreateTodoTaskCommand(CreateTodoTaskRequestDto requestDto)
    : IRequest<ApiResponse<string>>
{
    public CreateTodoTaskRequestDto RequestDto { get; } = requestDto;

    public class Handler(
        ITodoTaskRepository todoRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<CreateTodoTaskCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            CreateTodoTaskCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var actionerId = (int)employeeId;
            var dto = command.RequestDto;
 
            DateOnly? dueDate = null;
            if (!string.IsNullOrWhiteSpace(dto.DueDate) &&
                DateOnly.TryParseExact(dto.DueDate, "yyyy-MM-dd", out var parsed))
            {
                dueDate = parsed;
            }
 
            var task = new TodoTask(employeeId, dto.TaskName, dueDate, dto.Priority, actionerId);
            await todoRepository.AddAsync(task);
 
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
        ITodoTaskRepository todoRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<UpdateTodoTaskCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            UpdateTodoTaskCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var dto = command.RequestDto;
 
            var task = await todoRepository.GetByIdAsync(command.TaskId);
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
 
            task.UpdateDetails(dto.TaskName, dueDate, dto.Priority, (int)employeeId);
            await todoRepository.UpdateAsync(task);
 
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
        ITodoTaskRepository todoRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<ToggleTodoTaskCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            ToggleTodoTaskCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
 
            var task = await todoRepository.GetByIdAsync(command.TaskId);
            if (task == null)
            {
                return ApiHelperResponse.Failed<string>("Task not found.");
            }
 
            if (task.EmployeeId != employeeId)
            {
                return ApiHelperResponse.Failed<string>("You are not authorized to update this task.");
            }
 
            task.ToggleCompleted((int)employeeId);
            await todoRepository.UpdateAsync(task);
 
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
        ITodoTaskRepository todoRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<DeleteTodoTaskCommand, ApiResponse<string>>
    {
        public async Task<ApiResponse<string>> Handle(
            DeleteTodoTaskCommand command,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
 
            var task = await todoRepository.GetByIdAsync(command.TaskId);
            if (task == null)
            {
                return ApiHelperResponse.Failed<string>("Task not found.");
            }
 
            if (task.EmployeeId != employeeId)
            {
                return ApiHelperResponse.Failed<string>("You are not authorized to delete this task.");
            }
 
            await todoRepository.DeleteAsync(task);
 
            return ApiHelperResponse.Success("Task deleted successfully.", "Success");
        }
    }
}




