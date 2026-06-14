using CSharpFunctionalExtensions;

using HRManagement.Application.Interfaces;
using HRManagement.Application.Mappings;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetEmployeeListQuery() : IRequest<Result<List<EmployeeListResponseDto>>>;

internal sealed class GetEmployeeListQueryHandler(
    IEmployeeRepository employeeRepository,
    ILogger<GetEmployeeListQueryHandler> logger) : IRequestHandler<GetSalesmanListQuery, Result<List<SalesmanListResponseDto>>>
{
    public async Task<Result<List<SalesmanListResponseDto>>> Handle(GetSalesmanListQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetEmployeeListQueryHandler));

        var data = await salesmanRepository.GetSalesmanListAsync(request.SearchText, cancellationToken);

        return Result.Success(data);
    }
}

public class GetEmployeeListQuery : IRequest<ApiResponse<List<EmployeeListResponseDto>>>
{
    public class Handler(IEmployeeRepository employeeRepository, IApplicationDbContext appDbContext) : IRequestHandler<GetEmployeeListQuery, ApiResponse<List<EmployeeListResponseDto>>>
    {
        public async Task<ApiResponse<List<EmployeeListResponseDto>>> Handle(GetEmployeeListQuery request, CancellationToken cancellationToken)
        {
            var employees =  await employeeRepository.GetAllEmployeesAsync();
            
            var lookups = await appDbContext.SystemLookups 
                .AsNoTracking() 
                .Where(x => x.IsActive && x.Category == "EMPLOYMENT_STATUS") 
                .ToListAsync(cancellationToken);

            var response = employees 
                .Select(employee => employee.ToEmployeeListResponse(lookups)) 
                .ToList();
            
            return ApiHelperResponse.Success("Employee List Showed Successfully", response);
        }
    }
}
