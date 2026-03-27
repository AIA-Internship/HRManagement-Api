using FluentValidation;
using HRManagement.Api.Application.Commands.Timesheet;

namespace HRManagement.Api.Application.Validators.Timesheet;

public class SubmitTimesheetValidator : AbstractValidator<SubmitTimesheetCommand>
{
    public SubmitTimesheetValidator()
    {
        RuleFor(x => x.RequestDto.Year)
            .GreaterThan(2000).WithMessage("Year must be greater than 2000.")
            .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Year value is not valid.");

        RuleFor(x => x.RequestDto.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
    }
}

public class ApproveTimesheetValidator : AbstractValidator<ApproveTimesheetCommand>
{
    public ApproveTimesheetValidator()
    {
        RuleFor(x => x.RequestDto.SubmissionId)
            .GreaterThan(0).WithMessage("A valid submission ID is required.");
    }
}

public class GiveRevisionValidator : AbstractValidator<GiveRevisionCommand>
{
    public GiveRevisionValidator()
    {
        RuleFor(x => x.RequestDto.SubmissionId)
            .GreaterThan(0).WithMessage("A valid submission ID is required.");

        RuleFor(x => x.RequestDto.OverallNote)
            .NotEmpty().WithMessage("Overall revision note is required.")
            .MaximumLength(1000).WithMessage("Overall note cannot exceed 1000 characters.");

        RuleForEach(x => x.RequestDto.DayComments).ChildRules(comment =>
        {
            comment.RuleFor(c => c.Date)
                .NotEmpty().WithMessage("Comment date is required.")
                .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Comment date must be in yyyy-MM-dd format.");

            comment.RuleFor(c => c.Comment)
                .NotEmpty().WithMessage("Comment text is required.")
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
        });
    }
}
