using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HRManagement.Domain.Models.Payload.EmployeeDtos.Commands.Dto;

public class UploadAttachmentDto
{
    [Required]
    public string DocumentType { get; set; } = string.Empty;

    [Required] public List<IFormFile> Files { get; set; } = new();
}

