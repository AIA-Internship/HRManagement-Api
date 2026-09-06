using HRManagement.Application.Features.Identity.Commands;
using HRManagement.Application.Features.Identity.Queries;
using HRManagement.Domain.Models.Payload;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class LoginController(ISender sender) : BaseApiController(sender)
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginPayload payload, CancellationToken ct)
    {
        var command = new LoginCommand(payload.Email, payload.Password, payload.RememberMe);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("verify-forgot")]
    public async Task<IActionResult> VerifyForgot([FromBody] VerifyForgotPayload payload, CancellationToken ct)
    {
        var command = new VerifyForgotQuery(payload);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordPayload payload, CancellationToken ct)
    {
        var command = new ResetPasswordCommand(payload);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }
}
