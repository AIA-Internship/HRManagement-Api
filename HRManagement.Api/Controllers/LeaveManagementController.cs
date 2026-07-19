using FluentValidation;
using HRManagement.Application.Features.Leave.Commands;
using HRManagement.Application.Features.Leave.Queries;
using HRManagement.Domain.Models.Response;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRManagement.Domain.Models.Payload;
using HRManagement.Application.Features.System.Commands;
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
            IEnumerable<IValidator> validators) : base(mediator, logger, validators)

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
        [HttpPost("{attachmentId}/attachments")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse>> UploadAttachments(
        [FromRoute] int attachmentId,
        [FromForm] UploadAttachmentPayload payload)
        {
            string objectName = nameof(UploadAttachments);

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized("UserId not found in token");

                int currentUserId = int.Parse(userIdClaim);

                _logger.LogInformation("AttachmentId = {AttachmentId}", attachmentId);
                _logger.LogInformation("DocumentType = {DocumentType}", payload.DocumentType);
                _logger.LogInformation("Files Count = {Count}", payload.Files?.Count ?? 0);

                if (payload.Files == null || payload.Files.Count <= 0)
                    return BadRequest(ApiResponse<object>.Fail("File tidak ditemukan."));

                var files = payload.Files;

                foreach (var file in files)
                {
                    _logger.LogInformation(
                        "File: {Name}, Size: {Size}, Type: {Type}",
                        file.FileName,
                        file.Length,
                        file.ContentType);
                }

                var fileDtos = files.Select(f => new FileItemDto(
                    f.OpenReadStream(),
                    f.FileName,
                    f.ContentType,
                    f.Length
                )).ToList();

                var uploadCommand = new FileUploadCommand(fileDtos);
                var uploadResult = await _mediator.Send(uploadCommand);

                if (uploadResult.IsFailure)
                {
                    _logger.LogError("FileUploadCommand failed: {Error}", uploadResult.Error);
                    return BadRequest(uploadResult.Error);
                }

                var uploadTasks = uploadResult.Value;

                _logger.LogInformation("Upload success. Uploaded count = {Count}", uploadTasks.Count);

                var command = new UploadLeaveAttachmentCommand(
                    attachmentId,
                    payload.DocumentType,
                    uploadTasks,
                    currentUserId);

                var response = await this.ValidateAndExecute(
                    command,
                    c => _mediator.Send(command));

                _logger.LogInformation("End {Service}.", objectName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service}", objectName);
                return BadRequest(ex.ToString());
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
            string objectName = nameof(getBySupervisorId);

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


        [Authorize]
        [HttpPost]
        [Route("create")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> createLeaveRequest([FromBody] CreateLeaveRequestDto content)
        {
            string objectName = nameof(createLeaveRequest);

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("UserId not found in token");

                content.RequesterId = int.Parse(userIdClaim);

                var command = new CreateLeaveRequestCommand(content);

                var response = await this
                    .ValidateAndExecute(command, c => _mediator.Send(command))
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
        [HttpPost]
        [Route("edit")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> editLeaveRequest([FromBody] UpdateLeaveRequestDto content)
        {
            string objectName = nameof(editLeaveRequest);

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("UserId not found in token");

                content.RequestId = int.Parse(userIdClaim);

                var command = new UpdateLeaveRequestCommand(content);

                var response = await this
                    .ValidateAndExecute(command, c => _mediator.Send(command))
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
        [Route("{id}/attachments")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> getLeaveAttachments([FromRoute] int id)
        {
            string objectName = nameof(getLeaveAttachments);

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized("UserId not found in token");

                int requesterId = int.Parse(userIdClaim);

                var query = new GetLeaveAttachmentsQuery(id);

                var response = await this
                    .ValidateAndExecute(query, c => _mediator.Send(query))
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
        [HttpDelete]
        [Route("{id}/attachments")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> DeleteAttachments([FromRoute] int id)
        {
            string objectName = nameof(DeleteAttachments);

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaim))
                    return Unauthorized("UserId not found in token");

                int requesterId = int.Parse(userIdClaim);

                var command = new DeleteLeaveAttachmentCommand(
                    id,
                    requesterId);

                var response = await this
                    .ValidateAndExecute(command, c => _mediator.Send(command))
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


        [Authorize]
        [HttpGet]
        [Route("get-by-leave-id/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> readByLeaveId([FromRoute] int id)
        {
            string objectName = nameof(readByLeaveId);

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized("UserId not found in token");

                int requesterId = int.Parse(userIdClaim);

                var query = new GetLeaveRequestByIdQuery(id, requesterId);

                var response = await this
                    .ValidateAndExecute(query, c => _mediator.Send(query))
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
            string objectName = nameof(getLeaveBalance);

            try
            {
                _logger.LogInformation("Start {Service}.", objectName);

                Console.WriteLine("===== CLAIMS =====");

                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"{claim.Type} = {claim.Value}");
                }

                Console.WriteLine("==================");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                Console.WriteLine("NameIdentifier = " + (userIdClaim ?? "NULL"));

                if (userIdClaim == null)
                    return Unauthorized("UserId not found in token");

                int userId = int.Parse(userIdClaim);

                var command = new getLeaveBalanceQuery(userId);

                var response = await this
                    .ValidateAndExecute(command, c => _mediator.Send(command))
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
                var command = new ApprovedLeaveRequestCommand(id, userId);
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

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized("UserId not found in token");

                int userId = int.Parse(userIdClaim);
                var command = new RejectedLeaveRequestCommand(id, userId);
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
