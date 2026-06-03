using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using HRManagement.Api.Application.Auth.DTOs;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Constants;
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
        IPasswordHasher passwordHasher,
        ILogger<Handler> logger) : IRequestHandler<LoginQuery, ApiResponse<TokenResponseDto>>
    {
        public async Task<ApiResponse<TokenResponseDto>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .Include(u => u.SystemRole)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .AsNoTracking() 
                .FirstOrDefaultAsync(u => u.EmployeeEmail.ToLower() == request.Email.ToLower(), cancellationToken);

            if (user == null)
            {
                throw new ApiException(
                    "Unauthorized", 
                    StatusCodes.Status401Unauthorized, 
                    ExceptionConstants.NotAuthorized 
                );
            }

            if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new ApiException(
                    "Unauthorized", 
                    StatusCodes.Status401Unauthorized, 
                    ExceptionConstants.NotAuthorized 
                );
            }

            var employeeName = await dbContext.Employees
                .AsNoTracking()
                .Where(e => e.EmployeeEmail.ToLower() == user.EmployeeEmail.ToLower())
                .Select(e => e.FullName)
                .FirstOrDefaultAsync(cancellationToken) ?? "Intern";
            {
                throw new ApiException(
                    "Unauthorized", 
                    StatusCodes.Status401Unauthorized, 
                    ExceptionConstants.NotAuthorized 
                );
            }

            if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new ApiException(
                    "Unauthorized", 
                    StatusCodes.Status401Unauthorized, 
                    ExceptionConstants.NotAuthorized 
                );
            }

            var roleName = user.SystemRole?.Name;
            var permissions = user.SystemRole?.RolePermissions
                .Select(rp => rp.Permission.Name)
                .ToList() ?? new List<string>();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                roleName = user.RoleId switch
                {
                    0 => "Supervisor",
                    1 => "Employee",
                    _ => user.RoleId.ToString()
                };
            }
            
            var token = GenerateToken(user, request.RememberMe, roleName, employeeName, permissions);
            return ApiHelperResponse.Success("Login successful", new TokenResponseDto { Token = token });
        }
        
        private string GenerateToken(User user, bool rememberMe, string roleName, string fullName, List<string> permissions)
        {
            var jwtKey = configuration["AppSetting:Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
            var jwtIssuer = configuration["AppSetting:Jwt:Issuer"];
            var jwtAudience = configuration["AppSetting:Jwt:AudienceWeb"];
            
            var durationString = configuration["AppSetting:Jwt:DurationInMinutes"] ?? "60";
            var durationInMinutes = int.Parse(durationString);
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.EmployeeEmail),
                new Claim(ClaimTypes.Name, fullName),
                new Claim("fullname", fullName),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("role_id", user.RoleId.ToString())
            };

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }
            
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
