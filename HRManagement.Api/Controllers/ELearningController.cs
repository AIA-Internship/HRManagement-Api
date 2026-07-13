using CSharpFunctionalExtensions;
using FluentValidation;
using HRManagement.Api.Application;
using HRManagement.Api.Application.Commands.ELearningCommands;
using HRManagement.Api.Application.Queries;
using HRManagement.Api.Application.Queries.ELearningQueries;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ELearningController : ControllerBase
{
    private readonly IMediator _mediator;
    public ELearningController(IMediator mediator) => _mediator = mediator;

    private IActionResult FromResult(Result<ApiResponse> result)
    {
        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return result.Value.IsError ? BadRequest(result.Value) : Ok(result.Value);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] int userId)
    {
        var result = await _mediator.Send(new GetInternDashboardProgressQuery(userId));
        return FromResult(result);
    }

    [HttpGet("modules")]
    public async Task<IActionResult> GetAvailableModules([FromQuery] int userId, [FromQuery] string status = "All", [FromQuery] string search = "")
    {
        var result = await _mediator.Send(new GetAvailableModulesQuery(userId, status, search));
        return FromResult(result);
    }

    [HttpGet("modules/{moduleId}")]
    public async Task<IActionResult> GetModuleById(int moduleId)
    {
        var result = await _mediator.Send(new GetModuleByIdQuery(moduleId));
        return FromResult(result);
    }

    [HttpGet("completed-count")]
    public async Task<IActionResult> GetCompletedModulesCount([FromQuery] int userId)
    {
        var result = await _mediator.Send(new GetCompletedModulesCountQuery(userId));
        return FromResult(result);
    }

    [HttpGet("total-count")]
    public async Task<IActionResult> GetTotalModulesCount([FromQuery] string role)
    {
        var result = await _mediator.Send(new GetTotalModulesCountQuery(role));
        return FromResult(result);
    }

    [HttpPost("mark-opened")]
    public async Task<IActionResult> MarkAsOpened([FromBody] MarkMaterialOpenedCommand command)
    {
        var success = await _mediator.Send(command);
        return success ? Ok(new { message = "Progress updated" }) : BadRequest();
    }

    [HttpPost("submit-quiz")]
    public async Task<IActionResult> SubmitQuiz([FromBody] HRManagement.Api.Application.Commands.ELearningCommands.SubmitQuizCommand command)
    {
        var result = await _mediator.Send(command);
        return FromResult(result);
    }

    [HttpPost("add-module")]
    public async Task<IActionResult> AddModule([FromBody] CreateModuleDto dto)
    {
        var result = await _mediator.Send(new CreateModuleCommand(dto));
        return FromResult(result);
    }

    [HttpPut("update-module")]
    public async Task<IActionResult> UpdateModule([FromBody] UpdateModuleDto dto)
    {
        var result = await _mediator.Send(new UpdateModuleCommand(dto));
        return FromResult(result);
    }

    [HttpDelete("delete-module")]
    public async Task<IActionResult> DeleteModule([FromBody] DeleteModuleDto dto)
    {
        var result = await _mediator.Send(new DeleteModuleCommand(dto));
        return FromResult(result);
    }

    [HttpPost("add-content")]
    [RequestSizeLimit(157_286_400)]
    [RequestFormLimits(MultipartBodyLengthLimit = 157_286_400)]
    public async Task<IActionResult> AddContent([FromForm] UploadModuleContentCommand command)
    {
        var result = await _mediator.Send(command);
        return FromResult(result);
    }

    [HttpGet("batches/{batchId}/modules")]
    public async Task<IActionResult> GetModulesByBatch(int batchId, [FromQuery] string search = "", [FromQuery] List<string>? roles = null)
    {
        var result = await _mediator.Send(new GetModulesByBatchQuery(batchId, search, roles ?? new List<string>()));
        return FromResult(result);
    }

    [HttpPost("create-quiz")]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizConfigurationDto dto)
    {
        var result = await _mediator.Send(new CreateQuizCommand(dto));
        return FromResult(result);
    }

    [HttpGet("content/{contentId}/download")]
    public async Task<IActionResult> DownloadContent(int contentId)
    {
        var result = await _mediator.Send(new GetModuleContentFileQuery(contentId));
        if (!result.Found) return NotFound(new { message = "Content not found" });

        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "elearning", result.StoredFileName);
        if (!System.IO.File.Exists(physicalPath)) return NotFound(new { message = "File not found on disk" });

        var contentTypeProvider = new FileExtensionContentTypeProvider();
        if (!contentTypeProvider.TryGetContentType(physicalPath, out var mimeType))
            mimeType = "application/octet-stream";

        return PhysicalFile(physicalPath, mimeType, result.DownloadFileName);
    }

    [HttpPost("copy-module")]
    public async Task<IActionResult> CopyModule([FromBody] CopyModuleDto dto)
    {
        var result = await _mediator.Send(new CopyModuleCommand(dto));
        return FromResult(result);
    }

    [HttpPost("create-program")]
    public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto dto)
    {
        var result = await _mediator.Send(new CreateProgramCommand(dto));
        return FromResult(result);
    }

    [HttpPost("create-batch")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateBatchDto dto)
    {
        var result = await _mediator.Send(new CreateBatchCommand(dto));
        return FromResult(result);
    }

    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups()
    {
        var result = await _mediator.Send(new GetGroupsQuery());
        return FromResult(result);
    }

    [HttpGet("programs")]
    public async Task<IActionResult> GetPrograms()
    {
        var result = await _mediator.Send(new GetProgramsQuery());
        return FromResult(result);
    }

    [HttpGet("programs/{programId}/batches")]
    public async Task<IActionResult> GetBatchesByProgram(int programId)
    {
        var result = await _mediator.Send(new GetBatchesByProgramQuery(programId));
        return FromResult(result);
    }

    [HttpGet("quiz/{quizId}/submissions")]
    public async Task<IActionResult> GetQuizSubmissions(int quizId)
    {
        var result = await _mediator.Send(new GetQuizSubmissionsQuery(quizId));
        return FromResult(result);
    }

    [HttpGet("submissions/{submissionId}")]
    public async Task<IActionResult> GetSubmissionDetail(int submissionId)
    {
        var result = await _mediator.Send(new GetSubmissionDetailQuery(submissionId));
        return FromResult(result);
    }

    [HttpGet("interns")]
    [HttpGet("programs/{programId}/interns")]
    public async Task<IActionResult> GetInterns(int programId, [FromQuery] int page = 1, [FromQuery] string search = "", [FromQuery] string role = "")
    {
        var result = await _mediator.Send(new GetInternListQuery(programId, page, search, role));
        return Ok(result);
    }

    [HttpPut("grade-submission")]
    public async Task<IActionResult> GradeSubmission([FromBody] SubmitQuizReviewCommand command)
    {
        var result = await _mediator.Send(command);
        return FromResult(result);
    }
}
