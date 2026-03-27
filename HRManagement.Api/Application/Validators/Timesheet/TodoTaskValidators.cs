using FluentValidation;
using HRManagement.Api.Application.Commands.Timesheet;

namespace HRManagement.Api.Application.Validators.Timesheet;

public class CreateTodoTaskValidator : AbstractValidator<CreateTodoTaskCommand>
{
    public CreateTodoTaskValidator()
    {
        RuleFor(x => x.RequestDto.TaskName)
            .NotEmpty().WithMessage("Task name is required.")
            .MaximumLength(300).WithMessage("Task name cannot exceed 300 characters.");

        RuleFor(x => x.RequestDto.DueDate)
            .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Due date must be in yyyy-MM-dd format.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.DueDate));

        RuleFor(x => x.RequestDto.Priority)
            .InclusiveBetween(0, 2).WithMessage("Priority must be 0 (Low), 1 (Medium), or 2 (High).");
    }
}

public class UpdateTodoTaskValidator : AbstractValidator<UpdateTodoTaskCommand>
{
    public UpdateTodoTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0).WithMessage("A valid task ID is required.");

        RuleFor(x => x.RequestDto.TaskName)
            .NotEmpty().WithMessage("Task name is required.")
            .MaximumLength(300).WithMessage("Task name cannot exceed 300 characters.");

        RuleFor(x => x.RequestDto.DueDate)
            .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Due date must be in yyyy-MM-dd format.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.DueDate));

        RuleFor(x => x.RequestDto.Priority)
            .InclusiveBetween(0, 2).WithMessage("Priority must be 0 (Low), 1 (Medium), or 2 (High).");
    }
}
