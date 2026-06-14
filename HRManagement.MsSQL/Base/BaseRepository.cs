using HRManagement.Domain.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Base;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly AppDbContext _sqldbContext; // Ubah ke protected agar bisa diakses child class
    protected readonly DbSet<T> _dbContext;
    private bool _disposed = false;

    public BaseRepository(AppDbContext sqldbContext)
    {
        _sqldbContext = sqldbContext;
        _dbContext = _sqldbContext.Set<T>(); // Inisialisasi DbSet
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(entity, cancellationToken);
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
            _sqldbContext?.Dispose();
            _disposed = true;
        }
    }
}