using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using HRManagement.Application.Interfaces;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Timesheet;



public class TodoTaskRepository(AppDbContext dbContext) 
    : TimesheetBaseRepository<TodoTask>(dbContext), ITodoTaskRepository
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


