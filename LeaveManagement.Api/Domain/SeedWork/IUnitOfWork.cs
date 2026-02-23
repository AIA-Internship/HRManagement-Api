namespace LeaveManagement.Api.Domain.SeedWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> CommitAsync();
    }
}
