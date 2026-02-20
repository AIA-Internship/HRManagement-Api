using CSharpFunctionalExtensions;

using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table;
using HRManagement.Api.Domain.SeedWork;

using MediatR;

using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace HRManagement.Api.Application.Queries
{
    public class LoginQuery(string userMobile) : IRequest<Result<ApiResponse>>
    {
        public string UserMobile { get; set; } = userMobile;
    }

    internal class LoginQueryHandler : IRequestHandler<LoginQuery, Result<ApiResponse>>
    {
        private readonly IAuthorizationRepository _authorizationRepository;
        private readonly ILogger<LoginQueryHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginQueryHandler(IAuthorizationRepository authorizationRepository
            , IConfiguration configuration
            , IHttpClientFactory httpClientFactory
            , ILogger<LoginQueryHandler> logger
            , IUnitOfWork unitOfWork)
        {
            _authorizationRepository = authorizationRepository;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ApiResponse>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(LoginQueryHandler));

            try
            {
                var data = await _authorizationRepository.GetUserByPhoneAsync(request.UserMobile);
                if (data.UserId > 0)
                {
                    var token = GenerateToken(data);

                    return ApiHelperResponse.Success(token);
                }
                else
                {
                    return ApiHelperResponse.Failed("Invalid user id and/or password");
                }
            }
            catch (Exception ex)
            {
                return ApiHelperResponse.Failed(ex.Message);
            }
        }

        private string GenerateToken(UserModel user)
        {
            var jwtSettings = _configuration.GetSection("AppSetting");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Jwt:Key"]!));

            // Claims berisi informasi tentang user
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.MemberId.ToString()!),
                new Claim(ClaimTypes.Email, user.UserEmail!),
                new Claim(ClaimTypes.Role, user.RoleId.ToString()!)
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Jwt:Issuer"],
                audience: jwtSettings["Jwt:AudienceWeb"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt16(jwtSettings["Jwt:DurationInMinutes"])), // Token berlaku selama 1 jam
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
