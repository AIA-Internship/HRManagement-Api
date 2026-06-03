using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.SeedWork;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories.Timesheet;



public class TodoTaskRepository(AppDbContext dbContext) 
    : BaseRepository<TodoTask>(dbContext), ITodoTaskRepository
{
    public async Task<List<TodoTask>> GetTodoTasksByEmployeeAsync(int employeeId)
    {
        return await dbContext.TodoTasks
            .AsNoTracking()
            .Where(t => t.EmployeeId == employeeId && !t.IsDeleted)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .ToListAsync();
    }
}
