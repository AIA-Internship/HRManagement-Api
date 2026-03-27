using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using MediatR;

using HRManagement.Api.Application.Queries;
using HRManagement.Api.Domain.Models.Response.Shared;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[ApiController]
[Route("api/lookups")]
public class LookupController : ControllerBase
{
    private readonly ILogger<EmployeeController> _logger;
    private readonly IMediator _mediator;

    public LookupController(ILogger<EmployeeController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }
    
    [HttpGet("{enumName}")]
    [ProducesResponseType(typeof(ApiResponse<List<SystemLookupDto>>), 200)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetEnumValues(string enumName)
    {
        string methodName = nameof(GetEnumValues);
        _logger.LogInformation("Start {Service}", methodName);
        
        var query = new GetLookupValuesQuery(enumName);
        var response = await _mediator.Send(query);
        
        _logger.LogInformation("End {Service}", methodName);
        return Ok(response);
    }
}