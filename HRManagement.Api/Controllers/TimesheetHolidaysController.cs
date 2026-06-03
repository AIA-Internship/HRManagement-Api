using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRManagement.Api.Application.Commands.Timesheet;
using HRManagement.Api.Application.Queries.Timesheet;
using HRManagement.Api.Application.TimesheetDtos.Commands.Dto;
using HRManagement.Api.Application.TimesheetDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using System.Security.Claims;

namespace HRManagement.Api.Controllers
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
        public async Task<ActionResult<ApiResponse<List<HolidayDto>>>> GetHolidays()
        {
            var query = new GetHolidayListQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("bulk")]
        public async Task<ActionResult<ApiResponse<string>>> BulkUpsertHolidays([FromBody] BulkUpsertHolidaysDto dto)
        {
            // Get user ID from claims or use a default for now
            long userId = 0;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out long parsedId)) userId = parsedId;

            var command = new BulkUpsertHolidaysCommand { Dto = dto, ActionerId = userId };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteHoliday(int id)
        {
            long userId = 0;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out long parsedId)) userId = parsedId;

            var command = new DeleteHolidayCommand { Id = id, ActionerId = userId };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}

