using HRManagement.Domain.Models.Tables;

namespace HRManagement.Domain.SeedWork;

/// <summary>
/// Professional Enterprise Grade Generic Repository Interface.
/// Handles standard boilerplate CRUD for ANY HRM module (Timesheet, Leave, Performance).
/// </summary>
/// <typeparam name="T">Must be a record that inherits from BaseTable (Audit-ready).</typeparam>
public interface IRepository<T> : IDisposable where T : BaseTable
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetActiveListAsync(); 
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}


