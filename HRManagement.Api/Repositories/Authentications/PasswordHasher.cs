using HRManagement.Api.Application.Interfaces;

namespace HRManagement.Api.Repositories.Authentications;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hashedPassword)
    {
        return  BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}