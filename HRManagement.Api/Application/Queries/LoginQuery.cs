using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using HRManagement.Api.Application.Auth.DTOs;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;

namespace HRManagement.Api.Application.Queries;

public class LoginQuery(string email, string password, bool rememberMe) : IRequest<ApiResponse<TokenResponseDto>>
{
    public string Email { get; } = email;
    public string Password { get; } = password;
    public bool RememberMe { get; set; } = rememberMe;

    public class Handler(
        IApplicationDbContext dbContext, 
        IConfiguration configuration,  
        IPasswordHasher passwordHasher) : IRequestHandler<LoginQuery, ApiResponse<TokenResponseDto>>
    {
        public async Task<ApiResponse<TokenResponseDto>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .AsNoTracking() 
                .FirstOrDefaultAsync(u => u.EmployeeEmail == request.Email, cancellationToken);

            if (user == null) throw new ApiException("Not found", (int)System.Net.HttpStatusCode.NotFound, "User not found");
            
            bool isValid = passwordHasher.Verify(request.Password, user.PasswordHash);
            if (!isValid) throw new ApiException("Unauthorized", (int)System.Net.HttpStatusCode.Unauthorized, "Invalid email or password");

            var roleName = await dbContext.SystemLookups
                .AsNoTracking()
                .Where(x => x.Category == "ROLE" && x.Value == user.Role && x.IsActive)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(roleName))
            {
                roleName = user.Role switch
                {
                    0 => "Supervisor",
                    1 => "Employee",
                    _ => user.Role.ToString()
                };
            }
            
            var token = GenerateToken(user, request.RememberMe, roleName);
            
            return ApiHelperResponse.Success("Login successful", new TokenResponseDto { Token = token });
        }
        
        private string GenerateToken(User user, bool rememberMe, string roleName)
        {
            var jwtKey = configuration["AppSetting:Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
            var jwtIssuer = configuration["AppSetting:Jwt:Issuer"];
            var jwtAudience = configuration["AppSetting:Jwt:AudienceWeb"];
            
            var durationString = configuration["AppSetting:Jwt:DurationInMinutes"] ?? "60";
            var durationInMinutes = int.Parse(durationString);
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.EmployeeEmail),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("role_id", user.Role.ToString())
            };
            
            var expirationTime = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(durationInMinutes);

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
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
}
