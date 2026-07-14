namespace HRManagement.Domain.Models.Response;

public record UploadFileResponseDto
(
    string FileName,
    string FileUrl,
    string ContentType,
    long FileSize
);
