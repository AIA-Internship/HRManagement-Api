using HRManagement.Api.Domain.Models.Table;

namespace HRManagement.Api.Domain.Interfaces
{
    public interface IAuthorizationRepository
    {
        public Task<UserModel> GetUserByPhoneAsync(string userMobile);

        public Task AddLoginActivityAsync(LoginActivityModel entity);

        public Task<LoginActivityModel> GetVerificationAsync(string userMobile, string token);
        public Task<LoginActivityModel> GetTokenAsync(string userMobile);
    }
}
