using HRManagement.Domain.Interfaces;
using FluentValidation;
using HRManagement.Application.Commands.Timesheet;

namespace HRManagement.Application.Validators.Timesheet;

public class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.RequestDto.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters.");

        RuleFor(x => x.RequestDto.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => x.RequestDto.Description != null);
    }
}

public class UpdateProjectValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("A valid project ID is required.");

        RuleFor(x => x.RequestDto.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters.");

        RuleFor(x => x.RequestDto.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => x.RequestDto.Description != null);

        RuleFor(x => x.RequestDto.Status)
            .InclusiveBetween(0, 1).WithMessage("Status must be 0 (Running) or 1 (Finished).");
    }
}



