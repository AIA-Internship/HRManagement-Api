using HRManagement.Domain.Models.Response.Shared;

public interface IFileStorageService
{
    Task<string> UploadBlobAsync(
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<FileDownloadResult?> DownloadBlobAsync(
        string fileName,
        CancellationToken cancellationToken = default);
}