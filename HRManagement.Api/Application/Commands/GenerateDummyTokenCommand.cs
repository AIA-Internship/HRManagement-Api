using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace HRManagement.Api.Application.Commands
{
    public class GenerateDummyTokenCommand : IRequest<Result<ApiResponse>>
    {
        public int UserId { get; set; } = 1;
        public string Email { get; set; } = "dummy@test.com";
        public string RoleName { get; set; } = "Admin";
        public bool RememberMe { get; set; } = false;
    }
    internal class GenerateDummyTokenCommandHandler (IConfiguration configuration) : IRequestHandler<GenerateDummyTokenCommand, Result<ApiResponse>>
    {

        public async Task<Result<ApiResponse>> Handle(GenerateDummyTokenCommand request, CancellationToken cancellationToken)
        {
            // DEV ONLY protection
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env != "Development")
            {
                return ApiHelperResponse.Failed("This endpoint is only available in Development environment");
            }

            var jwtKey = configuration["AppSetting:Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is missing");

            var jwtIssuer = configuration["AppSetting:Jwt:Issuer"];
            var jwtAudience = configuration["AppSetting:Jwt:AudienceWeb"];

            var durationString = configuration["AppSetting:Jwt:DurationInMinutes"] ?? "60";
            var durationInMinutes = int.Parse(durationString);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.UserId.ToString()),
                new Claim(ClaimTypes.Email, request.Email),
                new Claim(ClaimTypes.Role, request.RoleName),
                new Claim("role_id", "1")
            };

            var expirationTime = request.RememberMe
                ? DateTime.UtcNow.AddDays(7)
                : DateTime.UtcNow.AddMinutes(durationInMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expirationTime,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            var data = new
            {
                token = jwt,
                expires = expirationTime
            };

            return ApiHelperResponse.Success("Dummy token generated successfully", data);
        }
    }
}
