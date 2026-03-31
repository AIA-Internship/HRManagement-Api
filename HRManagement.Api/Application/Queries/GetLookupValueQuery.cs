using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Api.Application.Queries;

public record GetLookupValuesQuery(string Category) : IRequest<ApiResponse<List<SystemLookupDto>>>;

public class GetLookupValuesQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetLookupValuesQuery, ApiResponse<List<SystemLookupDto>>>
{
    public async Task<ApiResponse<List<SystemLookupDto>>> Handle(GetLookupValuesQuery request, CancellationToken cancellationToken)
    {
        var lookupData = await dbContext.SystemLookups
            .AsNoTracking()
            .Where(x => x.Category.ToLower() == request.Category.ToLower() && x.IsActive)
            .OrderBy(x => x.Value)
            .Select(x => new SystemLookupDto(
                x.Value, 
                x.DisplayName
            ))
            .ToListAsync(cancellationToken);
        
        if (!lookupData.Any())
        {
            var errorMessage = string.Format(ExceptionConstants.LookupCategoryNotFound, request.Category);
            throw new ApiException(
                "Not Found", 
                StatusCodes.Status404NotFound, 
                errorMessage
            );
        }

        return ApiHelperResponse.Success($"Retrieved {request.Category} successfully", lookupData);
    }
}