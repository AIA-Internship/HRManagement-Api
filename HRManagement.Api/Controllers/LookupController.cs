using HRManagement.Application.Features.Master.References.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/lookups")]
public class LookupController(ISender sender) : BaseApiController(sender)
{
    [HttpGet("{enumName}")]
    public async Task<IActionResult> GetEnumValues(string enumName, CancellationToken ct)
    {
        var query = new GetLookupValuesQuery(enumName);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }
}