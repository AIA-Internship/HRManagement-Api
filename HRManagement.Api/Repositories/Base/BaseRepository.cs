using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories.Base;

/// <summary>
/// Professional Enterprise Grade Generic Repository.
/// All HRM Modules (Timesheet, Leave, etc.) should inherit from this.
/// </summary>
public abstract class BaseRepository<T>(AppDbContext dbContext) : IRepository<T> where T : BaseTableModel
{
    protected readonly AppDbContext dbContext = dbContext;
    private bool _disposed = false;

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await dbContext.Set<T>().FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id && !e.IsDeleted);
    }

    public virtual async Task<List<T>> GetActiveListAsync()
    {
        return await dbContext.Set<T>().Where(e => !e.IsDeleted).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await dbContext.Set<T>().AddAsync(entity);
        await dbContext.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(T entity)
    {
        dbContext.Set<T>().Update(entity);
        await dbContext.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        entity.IsDeleted = true; // Enterprise Soft-Delete standard
        await dbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            dbContext?.Dispose();
            _disposed = true;
        }
    }

    // Helper Utility for Employee Data
    public string NormalizePhoneNumber(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        string cleanNumber = input.Replace(" ", "").Replace("-", "").Trim();
        if (cleanNumber.StartsWith("+62")) return cleanNumber.Substring(3);
        if (cleanNumber.StartsWith("62")) return cleanNumber.Substring(2);
        if (cleanNumber.StartsWith("0")) return cleanNumber.Substring(1);
        return cleanNumber;
    }
}
