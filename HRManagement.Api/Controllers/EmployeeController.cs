using CSharpFunctionalExtensions;

using HRManagement.Application.Auth.Permissions;
using HRManagement.Application.Features.ESS.Employee.Commands;
using HRManagement.Application.Features.ESS.Employee.Queries;
using HRManagement.Application.Features.System.Commands;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Response.Shared;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/employee")]
public class EmployeeController(ISender sender) : BaseApiController(sender)
{
    [HttpGet("list")]
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetAllEmployeesAsync(CancellationToken ct)
    {
        var query = new GetEmployeeListQuery();
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfileAsync(CancellationToken ct)
    {
        var query = new GetMyProfileQuery(CurrentUserEmail);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("my-requests")]
    [HasPermission(Permissions.Employees.View)]
    public async Task<IActionResult> GetMyRequestsAsync([FromQuery] int? status, CancellationToken ct)
    {
        var query = new GetMyUpdateRequestQuery(CurrentEmployeeId, status);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("requests")]
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetEmployeeRequestsAsync([FromQuery] int? status, CancellationToken ct)
    {
        var query = new GetListUpdateRequestQuery(status);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("{displayId}")]
    [HasPermission(Permissions.Employees.View)]
    public async Task<IActionResult> GetProfileByDisplayIdAsync(string displayId, CancellationToken ct)
    {
        var query = new GetProfileByDisplayIdQuery(displayId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("supervisors-lookup")]
    [HasPermission(Permissions.Employees.View)]
    public async Task<IActionResult> GetSupervisorLookupAsync(CancellationToken ct)
    {
        var query = new GetSupervisorLookupQuery();
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }


    [HttpPut("employment-info/{displayId}")]
    [HasPermission(Permissions.Users.Edit)]
    public async Task<IActionResult> UpdateEmploymentInformationAsync(string displayId, [FromBody] UpdateEmploymentInfoPayload payload, CancellationToken ct)
    {
        var command = new UpdateEmployeeInfoCommand(displayId, payload, CurrentUserId);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }
    
    [HttpPut("me")]
    public async Task<IActionResult> UpdateEmployeeAsync([FromBody] UpdateEmployeePayload payload, CancellationToken ct)
    {
        var command = new UpdateEmployeeRequestCommand(payload, CurrentUserEmail, CurrentUserId);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }
    
    [HttpPost("review-update")]
    [HasPermission(Permissions.Employees.Edit)]
    public async Task<IActionResult> ReviewUpdateAsync([FromBody] ReviewUpdatePayload payload, CancellationToken ct)
    {
        var command = new ReviewUpdateEmployeeRequestCommand(payload, CurrentUserId);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }
    
    [HttpPost("create")]
    [HasPermission(Permissions.Employees.Create)]
    public async Task<IActionResult> CreateEmployeeAsync([FromBody] AddEmployeePayload payload, CancellationToken ct)
    {
        var command = new CreateEmployeeCommand(payload, CurrentUserId);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }

    [HttpPost("{id}/attachments")]
    [Consumes( "multipart/form-data")]
    public async Task<IActionResult> UploadAttachmentsAsync(int id, [FromForm] UploadAttachmentPayload payload, CancellationToken ct)
    {
        if (payload.Files.Count <= 0)
            return BadRequest(ApiResponse<object>.Fail("File tidak ditemukan."));

        var files = payload.Files;
        var fileDtos = files.Select(f => new FileItemDto(
            f.OpenReadStream(),
            f.FileName,
            f.ContentType,
            f.Length
        )).ToList();

        var uploadCommand = new FileUploadCommand(fileDtos);
        var uploadResult = await Sender.Send(uploadCommand, ct);

        if (uploadResult.IsFailure)
            return BadRequest(uploadResult.Error);

        var uploadTasks = uploadResult.Value;

        var command = new UploadAttachmentCommand(id, payload.DocumentType, uploadTasks, CurrentUserId);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }
}
