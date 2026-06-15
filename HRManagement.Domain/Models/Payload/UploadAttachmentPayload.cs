using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;

namespace HRManagement.Domain.Models.Payload;

public class UploadAttachmentPayload
{
    [Required]
    public string DocumentType { get; set; } = string.Empty;

    [Required] public List<IFormFile> Files { get; set; } = new();
}