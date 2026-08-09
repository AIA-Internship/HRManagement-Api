using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using Microsoft.Extensions.Options;

namespace HRManagement.Application.Services;

public sealed class FileStorageService(
    BlobServiceClient blobServiceClient,
    IOptions<AzureStorageOptions> options)
    : IFileStorageService
{
    public async Task<string> UploadBlobAsync(
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            blobServiceClient.GetBlobContainerClient(
                options.Value.ContainerName);

        var blobClient = containerClient.GetBlobClient(fileName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };

        await blobClient.UploadAsync(
            content,
            uploadOptions,
            cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<FileDownloadResult?> DownloadBlobAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            blobServiceClient.GetBlobContainerClient(
                options.Value.ContainerName);

        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync(cancellationToken))
            return null;

        var response = await blobClient.DownloadContentAsync(
            cancellationToken);

        return new FileDownloadResult
        {
            Content = response.Value.Content.ToArray(),
            ContentType =
                response.Value.Details.ContentType
                ?? "application/octet-stream"
        };
    }
}

public class AzureStorageOptions
{
    public const string SectionName = "AppSetting:AzureStorage";

    public string ContainerName { get; set; } = string.Empty;
}