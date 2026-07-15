using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables.ELearningModels;
using HRManagement.MsSQL.Base;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Application.Commands.ELearningCommands
{
    public class UploadModuleContentCommand : IRequest<Result>
    {
        public int ModuleId { get; set; }
        public string Title { get; set; } = null!;
        public IFormFile FilePayload { get; set; } = null!;
        public int CurrentUserId { get; set; }
    }

    internal class UploadModuleContentHandler : IRequestHandler<UploadModuleContentCommand, Result>
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private const long MaxFileSizeBytes = 150 * 1024 * 1024;

        public UploadModuleContentHandler(AppDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result> Handle(UploadModuleContentCommand request, CancellationToken ct)
        {
            if (request.FilePayload.Length > MaxFileSizeBytes)
                return Result.Failure("File size violates limits. Maximum allowed is 150MB.");

            var extension = Path.GetExtension(request.FilePayload.FileName).ToLower();
            if (extension != ".pdf" && extension != ".ppt" && extension != ".pptx" &&
                extension != ".mp4" && extension != ".docx")
            {
                return Result.Failure("Unsupported file format. Use PDF, PPT, MP4, or DOCX.");
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            using var stream = request.FilePayload.OpenReadStream();
            var blobUrl = await _fileStorageService.UploadBlobAsync(
                uniqueFileName,
                stream,
                request.FilePayload.ContentType,
                ct);

            var cleanContentType = extension.Replace(".", "");

            var newContent = new ModuleContentModel
            {
                ModuleId = request.ModuleId,
                ContentTitle = request.Title,
                ContentType = cleanContentType,

                ContentUrl = blobUrl,

                IsDeleted = false,
                CreatedBy = request.CurrentUserId.ToString(),
                CreatedUtcDate = DateTime.UtcNow
            };

            _context.ELearningModuleContents.Add(newContent);
            await _context.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}