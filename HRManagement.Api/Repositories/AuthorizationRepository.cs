using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories
{
    public class AuthorizationRepository : BaseRepository, IAuthorizationRepository
    {
        private readonly AppDbContext _dbContext; 

        public AuthorizationRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            string cleanEmail = email.Trim().ToLower();
            
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmployeeEmail.ToLower() == cleanEmail && !u.IsDeleted);

            return user;
        }
    }
}