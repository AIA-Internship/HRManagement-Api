using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using MediatR;

using HRManagement.Api.Application.Queries;
using HRManagement.Api.Domain.Models.Responses.Shared;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[ApiController]
[Route("api/lookups")]
public class LookupController(IMediator mediator) : ControllerBase
{
    [HttpGet("{enumName}")]
    [ProducesResponseType(typeof(ApiResponse<List<SystemLookupDto>>), 200)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetEnumValues(string enumName)
    {
        var query = new GetLookupValuesQuery(enumName);
        var response = await mediator.Send(query);
        
        return Ok(response);
    }
}