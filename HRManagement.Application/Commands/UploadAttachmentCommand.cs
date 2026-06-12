using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Application.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace HRManagement.Application.Commands;

public record UploadAttachmentCommand(int Id, string DocumentType, List<IFormFile> Files) : IRequest<ApiResponse>;

public class UploadEmployeeAttachmentsHandler : IRequestHandler<UploadAttachmentCommand, ApiResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _service;

    public UploadEmployeeAttachmentsHandler(IApplicationDbContext context, ICurrentUserService service)
    {
        _context = context;
        _service = service;
    }

    public async Task<ApiResponse> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        var employeeExists = await _context.Employees.AnyAsync(e => e.Id == request.Id, cancellationToken);
        if (!employeeExists) 
        {
            throw new ApiException(
                "Not Found", 
                StatusCodes.Status404NotFound, 
                ExceptionConstants.EmployeeNotFound
            );
        }
        
        bool isSupervisor = _service.Role == "Supervisor" || _service.Role == "0";
        if (!isSupervisor && _service.UserId != request.Id)
        {
            throw new ApiException(
                "Forbidden",
                StatusCodes.Status403Forbidden,
                ExceptionConstants.ForbiddenUpload
            );
        }

        var allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg" };
        var maxFileSize = 5 * 1024 * 1024;
        
        if (request.Files == null || request.Files.Count == 0)
        {
            throw new ApiException(
                "Bad Request", 
                StatusCodes.Status400BadRequest,
                ExceptionConstants.BadRequestUpload
            );
        }

        foreach (var file in request.Files)
        {
            if (file.Length == 0) 
                throw new ApiException(
                    "Bad Request",
                    StatusCodes.Status400BadRequest,
                    $"File {file.FileName} is empty."
                );
                
            if (file.Length > maxFileSize)
                throw new ApiException(
                    "Bad Request",
                    StatusCodes.Status400BadRequest,
                    $"File {file.FileName} exceeds the 5MB limit."
                );

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext)) 
                throw new ApiException(
                    "Bad Request",
                    StatusCodes.Status400BadRequest,
                    $"File type {ext} is not allowed."
                );
        }

        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

        var attachments = new List<EmployeeAttachment>();

        foreach (var file in request.Files)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            attachments.Add(new EmployeeAttachment
            {
                DocumentType = request.DocumentType,
                FileName = file.FileName,
                FilePath = $"/uploads/{uniqueFileName}",
                ContentType = file.ContentType,
                FileSize = file.Length,
                CreatedBy = request.Id
            });
        }

        await _context.EmployeeAttachments.AddRangeAsync(attachments, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return ApiHelperResponse.Success("Files uploaded successfully.");
    }
}