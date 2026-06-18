using HRManagement.Domain.Models.Tables;

namespace HRManagement.Domain.Interfaces;

public interface IUserRepository : IBaseRepository<Users>
{
    Task<Users?> GetUserByEmailAsync(string email, CancellationToken ct);
    Task<bool> IsUserVerifiedAsync(string email, DateTime dateOfBirth, CancellationToken ct);
}
