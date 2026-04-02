using FluentValidation;
using HRManagement.Api.Application.Commands;

namespace HRManagement.Api.Application.Validators;

public class UploadAttachmentValidator : AbstractValidator<UploadAttachmentCommand>
{
    public UploadAttachmentValidator()
    {
        RuleFor(x => x.Files)
            .NotEmpty().WithMessage("No files selected.");
            
        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Document type is required.");
    }
}