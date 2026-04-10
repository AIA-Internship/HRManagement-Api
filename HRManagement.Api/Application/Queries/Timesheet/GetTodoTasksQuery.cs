using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;

namespace HRManagement.Api.Application.Queries.Timesheet;

/// <summary>
/// Returns to-do tasks for the currently logged-in employee.
/// </summary>
public class GetTodoTasksQuery : IRequest<ApiResponse<List<TodoTaskResponseDto>>>
{
    public class Handler(
        ITimesheetRepository timesheetRepository,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetTodoTasksQuery, ApiResponse<List<TodoTaskResponseDto>>>
    {
        public async Task<ApiResponse<List<TodoTaskResponseDto>>> Handle(
            GetTodoTasksQuery request,
            CancellationToken cancellationToken)
        {
            var employeeId = currentUserService.UserId;
            var tasks = await timesheetRepository.GetTodoTasksByEmployeeAsync(employeeId);

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
