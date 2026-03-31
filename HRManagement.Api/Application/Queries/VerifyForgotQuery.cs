using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public record VerifyForgotQuery(VerifyForgotRequestDto Request) : IRequest<ApiResponse>;

public class VerifyForgotHandler : IRequestHandler<VerifyForgotQuery, ApiResponse>
{
    private readonly IApplicationDbContext _context; 
    
    public VerifyForgotHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse> Handle(VerifyForgotQuery request, CancellationToken cancellationToken)
    {
        var userExist = await _context.Employees
            .AnyAsync(e => e.EmployeeEmail == request.Request.Email &&
                           e.DateOfBirth.Date == request.Request.DateOfBirth.Date, cancellationToken); 
        if (!userExist) return ApiHelperResponse.Failed("Verification failed. Information does not match our records.");
        return ApiHelperResponse.Success("Identity verified.");
    }
}