using CSharpFunctionalExtensions;
using FluentValidation;
using HRManagement.Api.Application.Queries.LeaveManagementQueries;
using HRManagement.Api.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers
{
    [Route("api/leave")]
    [ApiController]
    public class LeaveManagementController : ValidationController<LeaveManagementController>
    {
        private readonly ILogger<LeaveManagementController> _logger;

        private readonly IMediator _mediator;

        public LeaveManagementController(
            ILogger<LeaveManagementController> logger,
            IMediator mediator,
            IEnumerable<IValidator> validators) : base(validators, logger)

        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        [Route("get-by-requester-id/{requesterId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]

        public async Task<ActionResult<ApiResponse>> getByRequesterId([FromRoute] int requesterId)
        {
            string objectName = nameof(getByRequesterId).ToString();
            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var query = new GetLeaveRequestByRequesterQuery(requesterId);
                var response = await this.ValidateAndExecute(query, (c) => _mediator.Send(query)).ConfigureAwait(false);

                _logger.LogInformation("End {Service}.", objectName);

                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in {Service}.", objectName);
                return BadRequest(ex.Message);
            }
        }

    }
}
