namespace HRManagement.Domain.Interfaces;

public interface IBaseRepository<T> : IDisposable where T : class
{
    Task<T?> GetByIdAsync(string? id, CancellationToken cancellationToken = default);
    Task<T?> GetByReqByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
}

