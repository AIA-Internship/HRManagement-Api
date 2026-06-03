using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;

namespace HRManagement.Api.Application.Queries.Timesheet
{
    public class GetHolidayListQuery : IRequest<ApiResponse<List<HolidayDto>>>
    {
    }

    public class GetHolidayListQueryHandler : IRequestHandler<GetHolidayListQuery, ApiResponse<List<HolidayDto>>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetHolidayListQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<HolidayDto>>> Handle(GetHolidayListQuery request, CancellationToken cancellationToken)
        {
            var holidays = await _dbContext.TimesheetHolidays
                .AsNoTracking()
                .OrderBy(x => x.HolidayDate)
                .Select(x => new HolidayDto
                {
                    Id = x.Id,
                    HolidayDate = x.HolidayDate,
                    HolidayName = x.Name,
                    Description = x.Description
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<HolidayDto>>.Success("Holidays retrieved successfully.", holidays);
        }
    }
}
