using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using HRManagement.Api.Repositories.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Commands.ELearningCommands
{
    public class UploadModuleContentCommand : IRequest<Result<ApiResponse>>
    {
        public int ModuleId { get; set; }
        public string Title { get; set; } = null!;
        public IFormFile FilePayload { get; set; } = null!;
        public int CurrentUserId { get; set; }
    }

    internal class UploadModuleContentHandler : IRequestHandler<UploadModuleContentCommand, Result<ApiResponse>>
    {
        private readonly SqlDbContext _context;
        private const long MaxFileSizeBytes = 150 * 1024 * 1024; // Strict 150MB layout rule

        public UploadModuleContentHandler(SqlDbContext context) => _context = context;

        public async Task<Result<ApiResponse>> Handle(UploadModuleContentCommand request, CancellationToken ct)
        {
            if (request.FilePayload.Length > MaxFileSizeBytes)
                return ApiHelperResponse.Failed("File size violates limits. Maximum allowed is 150MB.");

            var extension = Path.GetExtension(request.FilePayload.FileName).ToLower();
            if (extension != ".pdf" && extension != ".ppt" && extension != ".pptx" &&
                extension != ".mp4" && extension != ".docx")
            {
                return ApiHelperResponse.Failed("Unsupported file format. Use PDF, PPT, MP4, or DOCX.");
            }

            var baseStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "elearning");
            if (!Directory.Exists(baseStoragePath)) Directory.CreateDirectory(baseStoragePath);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var absoluteFilePath = Path.Combine(baseStoragePath, uniqueFileName);

            using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
            {
                await request.FilePayload.CopyToAsync(stream, ct);
            }

            var cleanContentType = extension.Replace(".", ""); // e.g. "mp4"

            var newContent = new ModuleContentModel
            {
                ModuleId = request.ModuleId,
                ContentTitle = request.Title,
                ContentType = cleanContentType,

                ContentUrl = uniqueFileName,

                IsDeleted = false,
                CreatedBy = request.CurrentUserId.ToString(),
                CreatedUtcDate = DateTime.UtcNow
            };

            _context.ELearningModuleContents.Add(newContent);
            await _context.SaveChangesAsync(ct);

            return ApiHelperResponse.Success("File content uploaded successfully.");
        }
    }
}