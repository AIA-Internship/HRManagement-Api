using CSharpFunctionalExtensions;

using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.System.Commands;

public record FileItemDto(Stream FileStream, string OriginalFileName, string ContentType, long Length);

public record FileUploadCommand(IEnumerable<FileItemDto> Files) : IRequest<Result<List<UploadFileResponseDto>>>;

internal sealed class FileUploadCommandHandler(
    IFileStorageService fileStorageService,
    ILogger<FileUploadCommandHandler> logger) : IRequestHandler<FileUploadCommand, Result<List<UploadFileResponseDto>>>
{
    public async Task<Result<List<UploadFileResponseDto>>> Handle(FileUploadCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(FileUploadCommandHandler));

        if (request.Files is null || !request.Files.Any())
        {
            logger.LogWarning("Upload file gagal: Daftar file kosong.");
            // Tipe kembalian sudah disesuaikan agar tidak compile-error
            return Result.Failure<List<UploadFileResponseDto>>("Daftar file yang diunggah tidak boleh kosong.");
        }

        var uploadTasks = new List<Task<UploadFileResponseDto>>();

        foreach (var file in request.Files)
        {
            // Validasi per file
            if (file.FileStream is null || file.FileStream.Length == 0)
            {
                logger.LogWarning("Upload dilewati untuk satu file: Stream kosong untuk file {FileName}", file.OriginalFileName);
                continue;
            }

            var fileExtension = Path.GetExtension(file.OriginalFileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            // 3. Daftarkan Task TANPA 'await' di sini agar berjalan di background (paralel)
            var uploadTask = UploadAndMapAsync(uniqueFileName, file, fileStorageService, cancellationToken);

            uploadTasks.Add(uploadTask);
        }

        if (uploadTasks.Count == 0)
            return Result.Failure<List<UploadFileResponseDto>>("Tidak ada file valid yang dapat diunggah.");

        try
        {
            // 4. Eksekusi semua Task upload secara bersamaan / paralel!
            var uploadedFiles = await Task.WhenAll(uploadTasks);

            return Result.Success(uploadedFiles.ToList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Terjadi kesalahan saat mengunggah kumpulan file.");
            return Result.Failure<List<UploadFileResponseDto>>("Gagal mengunggah satu atau lebih file ke penyimpanan.");
        }
    }

    private async Task<UploadFileResponseDto> UploadAndMapAsync(
        string uniqueFileName,
        FileItemDto file,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var uploadUrl = await fileStorageService.UploadBlobAsync(
            uniqueFileName,
            file.FileStream,
            file.ContentType,
            cancellationToken);

        return new UploadFileResponseDto(
            file.OriginalFileName,
            uploadUrl,
            file.ContentType,
            file.Length
        );
    }
}