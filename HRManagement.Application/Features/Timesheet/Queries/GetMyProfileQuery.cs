using HRManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using HRManagement.Application.Interfaces;
using AutoMapper;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Queries;

public class GetMyProfileQuery : IRequest<ApiResponse<EmployeeProfileResponseDto>>
{
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService, IMapper mapper, IApplicationDbContext appDbContext) : IRequestHandler<GetMyProfileQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<EmployeeProfileResponseDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var email = currentUserService.Email;
            if (string.IsNullOrEmpty(email))
            {
                throw new ApiException("Unauthorized", StatusCodes.Status401Unauthorized, ExceptionConstants.NotAuthorizedExcepction);
            }
        
            var profile = await employeeRepository.GetProfileByEmailAsync(email);
            if (profile == null)
            {
                throw new ApiException("Not found", StatusCodes.Status404NotFound, ExceptionConstants.EmployeeNotFound);
            }
            
            return ApiHelperResponse.Success("data retrieved successfully", profile);
        }
    }
}

