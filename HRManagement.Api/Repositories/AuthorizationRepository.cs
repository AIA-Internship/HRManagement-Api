using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Api.Domain.SeedWork;
using HRManagement.Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Repositories;

public class AuthorizationRepository(AppDbContext dbContext) 
    : BaseRepository<User>(dbContext), IAuthorizationRepository
{
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        string cleanEmail = email.Trim().ToLower();
        
        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.EmployeeEmail.ToLower() == cleanEmail && !u.IsDeleted);
    }
}