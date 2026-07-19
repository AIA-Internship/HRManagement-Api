using HRManagement.Application;
using HRManagement.Application.Commands.ELearningCommands;
using HRManagement.Application.Queries;
using HRManagement.Application.Queries.ELearningQueries;
using HRManagement.Domain.Models.Tables.ELearningModels.ELearningDto;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ELearningController(ISender sender) : BaseApiController(sender)
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] int userId)
    {
        var result = await Sender.Send(new GetInternDashboardProgressQuery(userId));
        return HandleResult(result);
    }

    [HttpGet("modules")]
    public async Task<IActionResult> GetAvailableModules([FromQuery] int userId, [FromQuery] string status = "All", [FromQuery] string search = "")
    {
        var result = await Sender.Send(new GetAvailableModulesQuery(userId, status, search));
        return HandleResult(result);
    }

    [HttpGet("modules/{moduleId}")]
    public async Task<IActionResult> GetModuleById(int moduleId)
    {
        var result = await Sender.Send(new GetModuleByIdQuery(moduleId));
        return HandleResult(result);
    }

    [HttpGet("completed-count")]
    public async Task<IActionResult> GetCompletedModulesCount([FromQuery] int userId)
    {
        var result = await Sender.Send(new GetCompletedModulesCountQuery(userId));
        return HandleResult(result);
    }

    [HttpGet("total-count")]
    public async Task<IActionResult> GetTotalModulesCount([FromQuery] string role)
    {
        var result = await Sender.Send(new GetTotalModulesCountQuery(role));
        return HandleResult(result);
    }

    [HttpPost("mark-opened")]
    public async Task<IActionResult> MarkAsOpened([FromBody] MarkMaterialOpenedCommand command)
    {
        var success = await Sender.Send(command);
        return success ? Ok(new { message = "Progress updated" }) : BadRequest();
    }

    [HttpPost("submit-quiz")]
    public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizCommand command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    [HttpPost("add-module")]
    public async Task<IActionResult> AddModule([FromBody] CreateModuleDto dto)
    {
        var result = await Sender.Send(new CreateModuleCommand(dto));
        return HandleResult(result);
    }

    [HttpPut("update-module")]
    public async Task<IActionResult> UpdateModule([FromBody] UpdateModuleDto dto)
    {
        var result = await Sender.Send(new UpdateModuleCommand(dto));
        return HandleResult(result);
    }

    [HttpDelete("delete-module")]
    public async Task<IActionResult> DeleteModule([FromBody] DeleteModuleDto dto)
    {
        var result = await Sender.Send(new DeleteModuleCommand(dto));
        return HandleResult(result);
    }

    [HttpDelete("delete-quiz")]
    public async Task<IActionResult> DeleteQuiz([FromBody] DeleteQuizDto dto)
    {
        var result = await Sender.Send(new DeleteQuizCommand(dto));
        return HandleResult(result);
    }

    [HttpDelete("delete-content")]
    public async Task<IActionResult> DeleteContent([FromBody] DeleteContentDto dto)
    {
        var result = await Sender.Send(new DeleteContentCommand(dto));
        return HandleResult(result);
    }

    [HttpPost("add-content")]
    [RequestSizeLimit(157_286_400)]
    [RequestFormLimits(MultipartBodyLengthLimit = 157_286_400)]
    public async Task<IActionResult> AddContent([FromForm] UploadModuleContentCommand command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    [HttpGet("batches/{batchId}/modules")]
    public async Task<IActionResult> GetModulesByBatch(int batchId, [FromQuery] string search = "", [FromQuery] List<string>? roles = null)
    {
        var result = await Sender.Send(new GetModulesByBatchQuery(batchId, search, roles ?? new List<string>()));
        return HandleResult(result);
    }

    [HttpPost("create-quiz")]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizConfigurationDto dto)
    {
        var result = await Sender.Send(new CreateQuizCommand(dto));
        return HandleResult(result);
    }

    [HttpGet("content/{contentId}/download")]
    public async Task<IActionResult> DownloadContent(int contentId)
    {
        var result = await Sender.Send(new GetModuleContentFileQuery(contentId));
        if (!result.Found) return NotFound(new { message = "Content not found" });

        return Redirect(result.FileUrl);
    }

    [HttpPost("copy-module")]
    public async Task<IActionResult> CopyModule([FromBody] CopyModuleDto dto)
    {
        var result = await Sender.Send(new CopyModuleCommand(dto));
        return HandleResult(result);
    }

    [HttpPost("create-program")]
    public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto dto)
    {
        var result = await Sender.Send(new CreateProgramCommand(dto));
        return HandleResult(result);
    }

    [HttpPost("create-batch")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateBatchDto dto)
    {
        var result = await Sender.Send(new CreateBatchCommand(dto));
        return HandleResult(result);
    }

    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups()
    {
        var result = await Sender.Send(new GetGroupsQuery());
        return HandleResult(result);
    }

    [HttpGet("programs")]
    public async Task<IActionResult> GetPrograms()
    {
        var result = await Sender.Send(new GetProgramsQuery());
        return HandleResult(result);
    }

    [HttpGet("programs/{programId}/batches")]
    public async Task<IActionResult> GetBatchesByProgram(int programId)
    {
        var result = await Sender.Send(new GetBatchesByProgramQuery(programId));
        return HandleResult(result);
    }

    [HttpGet("quiz/{quizId}/submissions")]
    public async Task<IActionResult> GetQuizSubmissions(int quizId)
    {
        var result = await Sender.Send(new GetQuizSubmissionsQuery(quizId));
        return HandleResult(result);
    }

    [HttpGet("submissions/{submissionId}")]
    public async Task<IActionResult> GetSubmissionDetail(int submissionId)
    {
        var result = await Sender.Send(new GetSubmissionDetailQuery(submissionId));
        return HandleResult(result);
    }

    [HttpGet("interns")]
    [HttpGet("programs/{programId}/interns")]
    public async Task<IActionResult> GetInterns(int programId, [FromQuery] int page = 1, [FromQuery] string search = "", [FromQuery] string role = "")
    {
        var result = await Sender.Send(new GetInternListQuery(programId, page, search, role));
        return Ok(result);
    }

    [HttpGet("interns/{employeeId}/programs/{programId}/module-progress")]
    public async Task<IActionResult> GetInternModuleProgress(int employeeId, int programId)
    {
        var result = await Sender.Send(new GetInternModuleProgressQuery(employeeId, programId));
        return HandleResult(result);
    }

    [HttpGet("interns/{employeeId}/batches/{batchId}/status")]
    public async Task<IActionResult> GetInternBatchStatus(int employeeId, int batchId)
    {
        var result = await Sender.Send(new GetInternBatchStatusQuery(employeeId, batchId));
        return HandleResult(result);
    }

    [HttpGet("interns/{employeeId}/programs/{programId}/quiz-progress")]
    public async Task<IActionResult> GetInternQuizProgress(int employeeId, int programId)
    {
        var result = await Sender.Send(new GetInternQuizProgressQuery(employeeId, programId));
        return HandleResult(result);
    }

    [HttpPut("grade-submission")]
    public async Task<IActionResult> GradeSubmission([FromBody] SubmitQuizReviewCommand command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }
}
