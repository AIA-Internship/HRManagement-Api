using HRManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System;

namespace HRManagement.Application.Queries.Timesheet;

public class HolidayQueries(int year, int month) : IRequest<ApiResponse<List<HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto.HolidayDto>>>
{
    public int Year { get; } = year;
    public int Month { get; } = month;

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<HolidayQueries, ApiResponse<List<HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto.HolidayDto>>>
    {
        public async Task<ApiResponse<List<HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto.HolidayDto>>> Handle(HolidayQueries request, CancellationToken cancellationToken)
        {
            var holidays = await dbContext.TimesheetHolidays
                .Where(h => h.HolidayDate.Year == request.Year && !h.IsDeleted)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync(cancellationToken);

            // AUTO-FETCH LOGIC (Di belakang langsung / Background Fetch)
            if (!holidays.Any())
            {
                try
                {
                    using var client = new HttpClient();
                    var url = $"https://date.nager.at/api/v3/PublicHolidays/{request.Year}/ID";
                    var response = await client.GetAsync(url, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        var document = JsonDocument.Parse(content);
                        
                        foreach (var element in document.RootElement.EnumerateArray())
                        {
                            if (DateTime.TryParse(element.GetProperty("date").GetString(), out var hDate))
                            {
                                string hName = element.GetProperty("localName").GetString() ?? element.GetProperty("name").GetString() ?? "Holiday";
                                var newHoliday = new HRManagement.Domain.Models.Tables.TimesheetHoliday(hDate, hName, "", 0);
                                dbContext.TimesheetHolidays.Add(newHoliday);
                                holidays.Add(newHoliday);
                            }
                        }
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                }
                catch (Exception) { /* If fetch fails, just proceed with empty list */ }
            }

            // Apply month filter if required
            if (request.Month > 0)
            {
                holidays = holidays.Where(h => h.HolidayDate.Month == request.Month).ToList();
            }

            var holidayDtos = holidays.Select(h => new HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto.HolidayDto
            {
                Id = h.Id,
                HolidayDate = h.HolidayDate,
                HolidayName = h.Name,
                Description = h.Description
            }).ToList();

            return ApiHelperResponse.Success<List<HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto.HolidayDto>>("Holidays retrieved successfully.", holidayDtos);
        }
    }
}
