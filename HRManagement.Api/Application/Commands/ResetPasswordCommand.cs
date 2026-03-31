using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Commands;

public record ResetPasswordCommand(ResetPasswordRequestDto request) : IRequest<ApiResponse>;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, ApiResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher; 
    
    public ResetPasswordHandler(IApplicationDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<ApiResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.EmployeeEmail == request.request.Email, cancellationToken);

        if (user == null) return ApiHelperResponse.Failed("User not found.");

        var newHashPassword = _hasher.Hash(request.request.NewPassword);
        
        user.ChangePassword(newHashPassword, user.Id);
        
        await _context.SaveChangesAsync(cancellationToken);
        return ApiHelperResponse.Success("Password reset successfully.");
    }
}