using HRManagement.Api.Domain.SeedWork;

namespace HRManagement.Api.Repositories.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool disposed = false;

        public UnitOfWork(SqlDbContext context)
        {
            Context = context;
        }

        public SqlDbContext Context { get; }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                return await Context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }


        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                Context?.Dispose();
                disposed = true;
            }
        }
    }
}
