using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRManagement.Application.Commands.Timesheet;
using HRManagement.Application.Queries.Timesheet;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Commands.Dto;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto;
using HRManagement.Domain.Models.Response.Shared;
using System.Security.Claims;
using System.Net.Http;
using System.Text.Json;

namespace HRManagement.Controllers
{
    // [Authorize] - Temporarily disabled for debugging/initial setup as per previous pattern
    [ApiController]
    [Route("api/timesheet/holidays")]
    public class TimesheetHolidaysController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TimesheetHolidaysController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<HolidayDto>>), 200)]
        public async Task<ActionResult<ApiResponse<List<HolidayDto>>>> GetHolidays([FromQuery] int year = 2026, [FromQuery] int month = 0)
        {
            var query = new HolidayQueries(year, month);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("bulk")]
        public async Task<ActionResult<ApiResponse<string>>> BulkUpsertHolidays([FromBody] BulkUpsertHolidaysDto dto)
        {
            // Get Users ID from claims or use a default for now
            long userId = 0;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out long parsedId)) userId = parsedId;

            var command = new BulkUpsertHolidaysCommand { Dto = dto, ActionerId = (int)userId };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteHoliday(int id)
        {
            long userId = 0;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out long parsedId)) userId = parsedId;

            var command = new DeleteHolidayCommand { Id = id, ActionerId = (int)userId };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("external")]
        public async Task<ActionResult<object>> FetchExternalHolidays([FromQuery] int year)
        {
            try
            {
                using var client = new HttpClient();
                
                // Using Nager.Date API - a fully free, public API that requires NO API Key
                var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/ID";
                var response = await client.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to fetch from external API.");
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<object>(content);
                return Ok(json);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}







