using HRManagement.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Base;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
    public void Dispose()
    {
        dbContext.Dispose();
    }
    public async ValueTask DisposeAsync()
    {
        await dbContext.DisposeAsync();
    }
    public async Task<T> ExecuteInStrategyAsync<T>(Func<Task<T>> operation)
    {
        // Fungsi ini akan memberitahu EF Core: 
        // "Tolong jalankan operasi ini, dan kalau koneksi database kedip, tolong retry otomatis."
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(operation);
    }
}