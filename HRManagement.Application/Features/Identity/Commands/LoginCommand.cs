using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Tables;
using HRManagement.Domain.SeedWork;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HRManagement.Application.Features.Identity.Commands;

public record LoginCommand(string Email, string Password, bool RememberMe) : IRequest<Result<TokenResponseDto>>;

internal sealed class LoginQueryHandler(
    IUserRepository userRepository,
    IEmployeeRepository employeeRepository,
    IConfiguration configuration,
    IUnitOfWork unitOfWork,
    ILogger<LoginQueryHandler> logger) : IRequestHandler<LoginCommand, Result<TokenResponseDto>>
{
    public async Task<Result<TokenResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 2. Hindari melog request object utuh jika berisi password!
        logger.LogInformation("Mencoba proses sign-in untuk email: {Email}", request.Email);

        // 3. Hapus ConfigureAwait(false) dan block Try-Catch
        var user = await userRepository.GetUserByEmailAsync(request.Email, cancellationToken);

        // 4. Perbaikan Keamanan: Pesan error disamakan agar tidak membocorkan apakah user terdaftar atau tidak
        if (user is null)
        {
            logger.LogWarning("Sign-in gagal: User tidak ditemukan ({UserMobile})", request.Email);
            return Result.Failure<TokenResponseDto>("User ID dan/atau password salah.");
        }

        //string pass = user.MobilePhone + request.Password;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Sign-in gagal: Password salah ({obj})", request.Email);
            return Result.Failure<TokenResponseDto>("User ID dan/atau password salah.");
        }

        // 5. Update state & commit
        user.SetLastLogin();
        await unitOfWork.CommitAsync(cancellationToken); // Passing cancellation token

        var profile = await employeeRepository.GetProfileByEmailAsync(request.Email, cancellationToken);
        string fullName = profile?.FullName ?? user.EmployeeEmail;

        string token = GenerateToken(user, fullName, request.RememberMe);

        return Result.Success(new TokenResponseDto(token));
    }

    private string GenerateToken(Users user, string fullName, bool rememberMe)
    {
        var jwtSettings = configuration.GetSection("AppSetting:Jwt");
        var keyString = jwtSettings["Key"];

        if (string.IsNullOrEmpty(keyString))
            throw new InvalidOperationException("JWT Key tidak ditemukan di konfigurasi.");

        var jwtIssuer = jwtSettings["Issuer"];
        var jwtAudience = jwtSettings["AudienceWeb"];
        var durationInMinutes = Convert.ToDouble(jwtSettings["DurationInMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("fullName", fullName),
                new Claim(ClaimTypes.Email, user.EmployeeEmail),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("EmployeeId", user.EmployeeId.ToString()),
                new Claim("RoleId", user.RoleId.ToString())
            };

        foreach (var permission in user.Role.RolePermission)
        {
            claims.Add(new Claim("Permission", permission.Permission.Name));
        }

        var expirationTime = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(durationInMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expirationTime,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
