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

public class GetEmployeeProfileByIdQuery(string DisplayId) : IRequest<ApiResponse<HRManagement.Domain.Models.Response.EmployeeProfileResponseDto>>
{
    public string DisplayId { get; } = DisplayId;
    
    public class Handler(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService, IMapper mapper, IApplicationDbContext appDbContext) : IRequestHandler<GetEmployeeProfileByIdQuery, ApiResponse<EmployeeProfileResponseDto>>
    {
        public async Task<ApiResponse<HRManagement.Domain.Models.Response.EmployeeProfileResponseDto>> Handle(GetEmployeeProfileByIdQuery request, CancellationToken cancellationToken)
        {
            var profile = await employeeRepository.GetProfileByDisplayIdAsync(request.DisplayId);
            if (profile == null)
            {
                throw new ApiException("Not found", StatusCodes.Status404NotFound, ExceptionConstants.EmployeeNotFound);
            }
            
            return ApiHelperResponse.Success("data retrieved successfully", profile);
        }
    }
}


