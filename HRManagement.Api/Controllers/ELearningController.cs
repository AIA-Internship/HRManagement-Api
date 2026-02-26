using FluentValidation;
using HRManagement.Api.Application;
using HRManagement.Api.Application.Commands;
using HRManagement.Api.Application.Commands.ELearningCommands;
using HRManagement.Api.Application.Queries;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
[ApiController]
[Route("api/[controller]")]
public class ELearningController : ControllerBase
{
    private readonly IMediator _mediator;
    public ELearningController(IMediator mediator) => _mediator = mediator;

    //Intern

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] int userId, [FromQuery] string role)
    {
        var result = await _mediator.Send(new GetInternDashboardQuery(userId, role));
        return Ok(result);
    }

    [HttpPost("mark-opened")]
    public async Task<IActionResult> MarkAsOpened([FromBody] MarkMaterialOpenedCommand command)
    {
        var success = await _mediator.Send(command);
        return success ? Ok(new { message = "Progress updated" }) : BadRequest();
    }

    [HttpPost("submit-quiz")]
    public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizCommand command)
    {
        var success = await _mediator.Send(command);
        return success ? Ok(new { message = "Quiz submitted successfully" }) : BadRequest();
    }

    //Supervisor


    [HttpPost("add-module")]
    public async Task<IActionResult> AddModule([FromBody] CreateModuleDto dto)
    {
        var command = new CreateModuleCommand(dto);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Value);
        }

        return Ok(result.Value);
    }

    [HttpPut("update-module")]
    public async Task<IActionResult> UpdateModule([FromBody] UpdateModuleCommand command)
    {
        var success = await _mediator.Send(command);
        return success ? Ok(new { message = "Module updated" }) : NotFound();
    }

    [HttpPost("add-content")]
    public async Task<IActionResult> AddContent([FromBody] AddContentCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { contentId = id, message = "Content added successfully" });
    }

    [HttpGet("interns")]
    public async Task<IActionResult> GetInterns([FromQuery] int page = 1, [FromQuery] string search = "")
    {
        var result = await _mediator.Send(new GetInternListQuery(page, search));
        return Ok(result);
    }

    [HttpPut("grade-submission")]
    public async Task<IActionResult> GradeSubmission([FromBody] GradeSubmissionCommand command)
    {
        var success = await _mediator.Send(command);
        return success ? Ok(new { message = "Grade saved" }) : NotFound();
    }
}