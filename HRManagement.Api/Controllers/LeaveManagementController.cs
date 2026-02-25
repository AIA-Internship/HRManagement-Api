using CSharpFunctionalExtensions;
using FluentValidation;
using HRManagement.Api.Application.Commands.LeaveManagementCommands;
using HRManagement.Api.Application.Queries.LeaveManagementQueries;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.LeaveManagementModel.LeaveRequest;
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
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]

        public async Task<ActionResult<ApiResponse>> getByRequesterId([FromRoute] int requesterId, [FromQuery] int max = 10)
        {
            string objectName = nameof(getByRequesterId).ToString();
            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var query = new GetLeaveRequestByRequesterQuery(requesterId, max);
                var response = await this.ValidateAndExecute(query, (c) => _mediator.Send(query)).ConfigureAwait(false);

                _logger.LogInformation("End {Service}.", objectName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service}.", objectName);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("create")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]

        public async Task<ActionResult<ApiResponse>> createLeaveRequest([FromBody] CreateLeaveRequestCommand content)
        {
            string objectName = nameof(createLeaveRequest).ToString();
            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var command = new CreateLeaveRequestCommand(content.LeaveRequestDto);
                var response = await this.ValidateAndExecute(command, (c) => _mediator.Send(command)).ConfigureAwait(false);

                _logger.LogInformation("End {Service}.", objectName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service}.", objectName);
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        [Route("edit")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]

        public async Task<ActionResult<ApiResponse>> editLeaveRequest([FromBody] UpdateLeaveRequestDto content)
        {
            string objectName = nameof(editLeaveRequest).ToString();
            try
            {
                _logger.LogInformation("Start {Service}.", objectName);
                var command = new UpdateLeaveRequestCommand(content);
                var response = await this.ValidateAndExecute(command, (c) => _mediator.Send(command)).ConfigureAwait(false);
                _logger.LogInformation("End {Service}.", objectName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service}.", objectName);
                return BadRequest(ex.Message);
            }

        }

        [HttpPut]
        [Route("soft-delete")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]

        public async Task<ActionResult<ApiResponse>> deleteLeaveRequest([FromBody] DeleteLeaveRequestDto content)
        {
            string objectName = nameof(editLeaveRequest).ToString();
            try
            {
                _logger.LogInformation("Start {Service}.", objectName);
                var command = new DeleteLeaveRequestCommand(content);
                var response = await this.ValidateAndExecute(command, (c) => _mediator.Send(command)).ConfigureAwait(false);
                _logger.LogInformation("End {Service}.", objectName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service}.", objectName);
                return BadRequest(ex.Message);
            }

        }

        [HttpGet]
        [Route("get-by-leave-id/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]

        public async Task<ActionResult<ApiResponse>> readByLeaveId([FromRoute] int id)
        {
            string objectName = nameof(editLeaveRequest).ToString();
            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var command = new GetLeaveRequestByIdQuery(id);
                var response = await this.ValidateAndExecute(command, (c) => _mediator.Send(command)).ConfigureAwait(false);

                _logger.LogInformation("End {Service}.", objectName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service}.", objectName);
                return BadRequest(ex.Message);
            }
        }
    }
}
