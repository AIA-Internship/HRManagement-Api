using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public record VerifyForgotQuery(VerifyForgotRequestDto request) : IRequest<ApiResponse>;

public class VerifyForgotHandler : IRequestHandler<VerifyForgotQuery, ApiResponse>
{
    private readonly IApplicationDbContext _context; 
    
    public VerifyForgotHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse> Handle(VerifyForgotQuery request, CancellationToken cancellationToken)
    {
        var UserExist = await _context.Employees
            .AnyAsync(e => e.EmployeeEmail == request.request.Email &&
                           e.DateOfBirth.Date == request.request.DateOfBirth.Date, cancellationToken); 
        if (!UserExist) return ApiHelperResponse.Failed("Verification failed. Information does not match our records.");
        return ApiHelperResponse.Success("Identity verified.");
    }
}