using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Tables;
using HRManagement.MsSQL.Base;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.MsSQL.Repositories;

public class UserRepository : BaseRepository<Users>, IUserRepository
{
    public UserRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<Users?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        string cleanEmail = email.Trim().ToLower();

        var user = await _dbContext
            .Include(u => u.Role) 
            .ThenInclude(r => r.RolePermissions) 
            .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .Where(u => u.EmployeeEmail.ToLower() == email.ToLower() && !u.IsDeleted)
            .FirstOrDefaultAsync(ct);

        return user;
    }

    public async Task<bool> IsUserVerifiedAsync(string email, DateTime dateOfBirth, CancellationToken ct)
    {
        string cleanEmail = email.Trim().ToLower();

        var result = await _dbContext
            .AsNoTracking()
            .Where(u => u.EmployeeEmail.ToLower() == email.ToLower() && !u.IsDeleted)
            .FirstOrDefaultAsync(ct);

        return result != null ? true : false;
    }
}
