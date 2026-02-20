using HRManagement.Api.Domain.SeedWork;

namespace HRManagement.Api.Repositories.Base
{
    public abstract class BaseRepository : IRepository
    {
        private SqlDbContext _sqldbContext;

        private bool _disposed = false;

        public BaseRepository(SqlDbContext sqldbContext)
        {
            _sqldbContext = sqldbContext;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _sqldbContext?.Dispose();
                _disposed = true;
            }
        }

        public string NormalizePhoneNumber(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // 1. Bersihkan spasi atau strip jika ada (opsional, tapi disarankan)
            string cleanNumber = input.Replace(" ", "").Replace("-", "").Trim();

            // 2. Cek Prefix dan sesuaikan
            if (cleanNumber.StartsWith("+62"))
            {
                // Hapus +62 (3 karakter)
                return cleanNumber.Substring(3);
            }
            else if (cleanNumber.StartsWith("62"))
            {
                // Hapus 62 (2 karakter)
                return cleanNumber.Substring(2);
            }
            else if (cleanNumber.StartsWith("0"))
            {
                // Hapus 0 (1 karakter)
                return cleanNumber.Substring(1);
            }

            // Jika input sudah 8xxx, kembalikan apa adanya
            return cleanNumber;
        }
    }
}
