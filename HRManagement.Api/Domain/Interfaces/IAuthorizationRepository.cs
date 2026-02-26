using HRManagement.Api.Domain.Models.Tables;

namespace HRManagement.Api.Domain.Interfaces
{
    public interface IAuthorizationRepository
    {
        public Task<User?> GetUserByEmailAsync(string email);
    }
}
