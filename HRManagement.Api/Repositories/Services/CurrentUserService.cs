using System.Security.Claims;
using HRManagement.Api.Application.Interfaces;

namespace HRManagement.Api.Repositories.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string? Email => _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

    public int UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(idClaim, out var userId))
            {
                return userId;
            }

            return 0;
        }
    }
}