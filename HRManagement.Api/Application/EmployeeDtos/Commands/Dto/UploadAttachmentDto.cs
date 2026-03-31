using System.ComponentModel.DataAnnotations;

namespace HRManagement.Api.Application.EmployeeDtos.Commands.Dto;

public class UploadAttachmentDto
{
    [Required]
    public string DocumentType { get; set; } = string.Empty;

    [Required] public List<IFormFile> Files { get; set; } = new();
}