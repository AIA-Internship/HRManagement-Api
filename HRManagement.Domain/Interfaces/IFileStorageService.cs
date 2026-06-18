namespace HRManagement.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadBlobAsync(
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);
}
