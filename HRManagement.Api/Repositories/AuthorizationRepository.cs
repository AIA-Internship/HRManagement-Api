using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Table;
using HRManagement.Api.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories
{
    public class AuthorizationRepository : BaseRepository, IAuthorizationRepository
    {
        private readonly SqlDbContext _dbContext;

        public AuthorizationRepository(SqlDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserModel> GetUserByPhoneAsync(string userMobile)
        {
            // 1. Normalisasi dulu input user menjadi format 'clean' (start 8xxx)
            string cleanPhone = NormalizePhoneNumber(userMobile);

            // 2. Siapkan variasi yang MUNGKIN tersimpan di database
            var searchVariations = new List<string>
            {
                cleanPhone,             // Cek format "8123..."
                "0" + cleanPhone,       // Cek format "08123..."
                "62" + cleanPhone,      // Cek format "628123..."
                "+62" + cleanPhone      // Cek format "+628123..."
            };

            var user = await _dbContext.User.FirstOrDefaultAsync(p => searchVariations.Contains(p.UserMobile) && !p.IsDeleted) ?? new();

            return user;
        }

        public async Task AddLoginActivityAsync(LoginActivityModel entity)
        {
            await _dbContext.LoginActivity.AddAsync(entity).ConfigureAwait(false);
        }

        public async Task<LoginActivityModel> GetTokenAsync(string userMobile)
        {
            return await _dbContext.LoginActivity.FirstOrDefaultAsync(p => p.UserMobile == userMobile && p.IsActive == true) ?? new();
        }

        public async Task<LoginActivityModel> GetVerificationAsync(string userMobile, string token)
        {
            return await _dbContext.LoginActivity.FirstOrDefaultAsync(p => p.UserMobile == userMobile && p.Token == token && p.IsActive == true) ?? new();
        }
    }
}
