namespace HRManagement.Domain.SeedWork;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task<T> ExecuteInStrategyAsync<T>(Func<Task<T>> operation);
}
