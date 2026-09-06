using HRManagement.Domain.Interfaces;
using FluentValidation;
using HRManagement.Application.Commands;

namespace HRManagement.Application.Validators;

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


