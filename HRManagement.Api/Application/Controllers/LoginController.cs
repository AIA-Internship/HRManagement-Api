using FluentValidation;

using HRManagement.Api.Application.Queries;
using HRManagement.Api.Domain.Models.Response.Shared;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginController : ValidationController<LoginController>
    {
        private readonly ILogger<LoginController> _logger;

        private readonly IMediator _mediator;

        public LoginController(
            ILogger<LoginController> logger,
            IMediator mediator,
            IEnumerable<IValidator> validators) : base(validators, logger)

        {
            _logger = logger;
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("sign-in")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ApiResponse>> LoginAsync([FromBody] string userMobile)
        {

            string objectName = nameof(LoginAsync).ToString();

            try
            {
                //_telemetryClient?.TrackEvent($"{objectName}", _trackProperties);
                _logger.LogInformation("Start {Service}.", objectName);

                var query = new LoginQuery(userMobile);
                var response = await this.ValidateAndExecute(query, (c) => _mediator.Send(query)).ConfigureAwait(false);

                _logger.LogInformation("End {Service}.", objectName);

                return response;
            }
            catch (Exception ex)
            {
                //_telemetryClient.TrackException(ex, _trackProperties);
                _logger.LogError(ex, "Error in {Service}.", objectName);
                return BadRequest(ex.Message);
            }
        }
    }
}
