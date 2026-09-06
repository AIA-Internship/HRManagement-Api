using HRManagement.Domain.Interfaces;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Application.Queries.Timesheet;

/// <summary>
/// Returns to-do tasks for the currently logged-in employee.
/// </summary>
public class GetTodoTasksQuery : IRequest<ApiResponse<List<TodoTaskResponseDto>>>
{
    public class Handler(
        ITodoTaskRepository todoRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetTodoTasksQuery, ApiResponse<List<TodoTaskResponseDto>>>
    {
        public async Task<ApiResponse<List<TodoTaskResponseDto>>> Handle(
            GetTodoTasksQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var tasks = await todoRepository.GetTodoTasksByEmployeeAsync(employeeId);

            var result = tasks.Select(t => new TodoTaskResponseDto
            {
                Id = t.Id,
                TaskName = t.TaskName,
                DueDate = t.DueDate?.ToString("yyyy-MM-dd"),
                Priority = t.Priority switch { 0 => "Low", 1 => "Medium", 2 => "High", _ => "Low" },
                IsCompleted = t.IsCompleted
            }).ToList();

            return ApiHelperResponse.Success("To-do tasks retrieved successfully.", result);
        }
    }
}




