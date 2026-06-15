using CSharpFunctionalExtensions;

using HRManagement.Domain.Models.Response.Shared;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace HRManagement.Api.Controllers;

[ApiController]
public abstract class BaseApiController(ISender sender) : ControllerBase
{
    protected readonly ISender Sender = sender;

    protected int CurrentUserId
    {
        get
        {
            var idString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(idString) || !short.TryParse(idString, out short userId))
            {
                // Jika token cacat atau tidak ada, lempar exception.
                // GlobalExceptionHandler Anda yang akan menangkap dan mengubahnya menjadi Error 500/401.
                throw new UnauthorizedAccessException("Sesi tidak valid atau User ID tidak ditemukan di dalam Token.");
            }

            return userId;
        }

    }

    protected int CurrentEmployeeId
    {
        get
        {
            var result = User.FindFirstValue("EmployeeId");

            if (string.IsNullOrEmpty(result) || !int.TryParse(result, out int employeeId))
                throw new UnauthorizedAccessException("Session tidak valid atau Attribute tidak ditemukan di dalam Token.");

            return employeeId;
        }

    }

    protected string CurrentUserEmail
    {
        get
        {
            var result = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(result))
                throw new UnauthorizedAccessException("Session tidak valid atau Attribute tidak ditemukan di dalam Token.");

            return result;
        }

    }

    protected string CurrentUserRole
    {
        get
        {
            var result = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(result))
                throw new UnauthorizedAccessException("Session tidak valid atau Attribute tidak ditemukan di dalam Token.");

            return result;
        }

    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            // Mengembalikan HTTP 200 + ApiResponse.StatusCode 200
            var response = ApiResponse<T>.Success(result.Value, "Sukses", StatusCodes.Status200OK);
            return Ok(response);
        }

        // Jika error, kita bisa menentukan HTTP Status berdasarkan isi pesan error bisnisnya
        // Contoh: Jika pesan error mengandung "tidak ditemukan", kita jadikan 404
        if (result.Error.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
        {
            var response = ApiResponse<T>.Fail(result.Error, StatusCodes.Status404NotFound, "Not Found");
            return NotFound(response);
        }

        // Default error bisnis adalah 400 Bad Request
        var badRequestResponse = ApiResponse<T>.Fail(result.Error, StatusCodes.Status400BadRequest, "Bad Request");
        return BadRequest(badRequestResponse);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            // Karena tidak ada kembalian data, kita isi Content dengan null
            var response = ApiResponse<object>.Success(null, "Berhasil diproses", StatusCodes.Status200OK);
            return Ok(response);
        }

        // Penanganan error (Sama seperti versi generic)
        if (result.Error.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
        {
            var response = ApiResponse<object>.Fail(result.Error, StatusCodes.Status404NotFound, "Not Found");
            return NotFound(response);
        }

        var badRequestResponse = ApiResponse<object>.Fail(result.Error, StatusCodes.Status400BadRequest, "Bad Request");
        return BadRequest(badRequestResponse);
    }
}
