using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;
using HRManagement.MsSQL.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Timesheet;

public class TimesheetBaseRepository<T>(AppDbContext dbContext) : IRepository<T> where T : BaseTable
{
    protected readonly AppDbContext dbContext = dbContext;

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await dbContext.Set<T>().FirstOrDefaultAsync(e => !e.IsDeleted && EF.Property<int>(e, "Id") == id);
    }

    public virtual async Task<List<T>> GetActiveListAsync()
    {
        return await dbContext.Set<T>().Where(e => !e.IsDeleted).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await dbContext.Set<T>().AddAsync(entity);
    }

    public virtual async Task UpdateAsync(T entity)
    {
        dbContext.Set<T>().Update(entity);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(T entity)
    {
        dbContext.Set<T>().Remove(entity);
        await Task.CompletedTask;
    }

    public virtual async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        dbContext.Dispose();
    }
}
