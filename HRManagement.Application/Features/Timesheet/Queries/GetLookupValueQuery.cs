using HRManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Queries.Dto;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Application.Queries;

public record GetLookupValuesQuery(string Category) : IRequest<ApiResponse<List<SystemLookupDto>>>;

public class GetLookupValuesQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetLookupValuesQuery, ApiResponse<List<SystemLookupDto>>>
{
    public async Task<ApiResponse<List<SystemLookupDto>>> Handle(GetLookupValuesQuery request, CancellationToken cancellationToken)
    {
        var lookupData = await dbContext.Lookup
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





