using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using HRManagement.Api.Application.Auth.DTOs;
using HRManagement.Api.Domain.Models.Response.Shared;

namespace HRManagement.Api.Application.Queries
{
    public class GenerateDummyTokenQuery : IRequest<Result<ApiResponse>>
    {
        public bool RememberMe { get; set; }

        public GenerateDummyTokenQuery(bool rememberMe = false)
        {
            RememberMe = rememberMe;
        }
    }

    internal class GenerateDummyTokenQueryHandler
        : IRequestHandler<GenerateDummyTokenQuery, Result<ApiResponse>>
    {
        private readonly IConfiguration _configuration;

        public GenerateDummyTokenQueryHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Result<ApiResponse>> Handle(
            GenerateDummyTokenQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 🔥 Dummy user data
                var dummyUserId = 999;
                var dummyEmail = "dummy@intern.com";
                var dummyRoleName = "Employee";
                var dummyRoleId = "1";

                var token = GenerateToken(
                    dummyUserId,
                    dummyEmail,
                    dummyRoleName,
                    dummyRoleId,
                    request.RememberMe
                );

                var dto = new TokenResponseDto
                {
                    Token = token
                };

                return ApiHelperResponse.Success("Dummy token generated", dto);
            }
            catch (Exception ex)
            {
                return ApiHelperResponse.Failed($"Failed to generate dummy token: {ex.Message}");
            }
        }

        private string GenerateToken(
            int userId,
            string email,
            string roleName,
            string roleId,
            bool rememberMe)
        {
            var jwtKey = _configuration["AppSetting:Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is missing");

            var jwtIssuer = _configuration["AppSetting:Jwt:Issuer"];
            var jwtAudience = _configuration["AppSetting:Jwt:AudienceWeb"];

            var durationString = _configuration["AppSetting:Jwt:DurationInMinutes"] ?? "60";
            var durationInMinutes = int.Parse(durationString);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("role_id", roleId)
            };

            var expirationTime = rememberMe
                ? DateTime.UtcNow.AddDays(7)
                : DateTime.UtcNow.AddMinutes(durationInMinutes);

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

