using CSharpFunctionalExtensions;
using FluentValidation;
using HRManagement.Api.Application.Commands.LeaveManagementCommands;
using HRManagement.Api.Application.Queries;
using HRManagement.Api.Application.Queries.LeaveManagementQueries;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables.LeaveManagementModel.LeaveRequest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManagement.Api.Controllers
{
    [Route("api/leave")]
    [ApiController]
    public class LeaveManagementController : ValidateController<LeaveManagementController>
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

        [Authorize]
        [HttpGet]
        [Route("get-by-requester-id")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> getByRequesterId([FromQuery] int max = 10)
        {
            string objectName = nameof(getByRequesterId).ToString();

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized("UserId not found in token");

                int requesterId = int.Parse(userIdClaim);

                var query = new GetLeaveRequestByRequesterQuery(requesterId, max);

                var response = await this
                    .ValidateAndExecute(query, (c) => _mediator.Send(query))
                    .ConfigureAwait(false);

                _logger.LogInformation("End {Service}.", objectName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service}.", objectName);
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("get-by-supervisor-id")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> getBySupervisorId([FromQuery] int max = 10)
        {
            string objectName = nameof(getBySupervisorId).ToString();

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized("UserId not found in token");

                int supervisorId = int.Parse(userIdClaim);

                var query = new GetLeaveRequestBySupervisorId(supervisorId, max);

                var response = await this
                    .ValidateAndExecute(query, (c) => _mediator.Send(query))
                    .ConfigureAwait(false);

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

        public async Task<ActionResult<ApiResponse>> createLeaveRequest([FromBody] CreateLeaveRequestDto content)
        {
            string objectName = nameof(createLeaveRequest).ToString();
            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var command = new CreateLeaveRequestCommand(content);
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


        [HttpGet]
        [Route("get-by-month")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]

        public async Task<ActionResult<ApiResponse>> readByMonth([FromQuery] int month, [FromQuery] int year)
        {
            string objectName = nameof(readByMonth).ToString();
            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var command = new GetLeaveRequestByMonthRangeQuery(year, month);
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



        [Authorize]
        [HttpGet]
        [Route("get-leave-balance")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> getLeaveBalance()
        {
            string objectName = nameof(getLeaveBalance).ToString();

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized("UserId not found in token");

                Console.WriteLine(userIdClaim);

                int userId = int.Parse(userIdClaim);

                var command = new getLeaveBalanceQuery(userId);

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

        [Authorize]
        [HttpGet]
        [Route("get-all-amount-type")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> getAllTypeAmount()
        {
            string objectName = nameof(getAllTypeAmount).ToString();

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized("UserId not found in token");

                int userId = int.Parse(userIdClaim);

                var command = new getEmployeeTypeAmountQuery(userId);

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

        [Authorize]
        [HttpPost]
        [Route("approve-request")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> approvedEmail([FromRoute] int id) { 
            string objectName = nameof(approvedEmail).ToString();

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized("UserId not found in token");

                int userId = int.Parse(userIdClaim);
                var command = new ApprovedLeaveRequestCommand(id);
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
        [Route("rejected-request/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> rejectedEmail([FromRoute] int id)
        {
            string objectName = nameof(rejectedEmail).ToString();


            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var command = new RejectedLeaveRequestCommand(id);
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
