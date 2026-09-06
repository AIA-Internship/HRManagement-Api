using HRManagement.Domain.Interfaces;
using FluentValidation;
using HRManagement.Application.Commands.Timesheet;

namespace HRManagement.Application.Validators.Timesheet;

public class SaveDailyTimesheetValidator : AbstractValidator<SaveDailyTimesheetCommand>
{
    public SaveDailyTimesheetValidator()
    {
        RuleFor(x => x.RequestDto.Date)
            .NotEmpty().WithMessage("Date is required.")
            .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Date must be in yyyy-MM-dd format.");

        RuleFor(x => x.RequestDto.Entries)
            .NotNull().WithMessage("Entries list cannot be null.");

        // Only require and validate entries if dayType is 'working'
        When(x => x.RequestDto.DayType == "working", () =>
        {
            RuleFor(x => x.RequestDto.Entries)
                .NotEmpty()
                .WithMessage("At least one entry row is required for working days.");

            RuleForEach(x => x.RequestDto.Entries)
                .ChildRules(entry =>
                {
                    entry.RuleFor(e => e.DurationMinutes)
                        .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes.")
                        .LessThanOrEqualTo(1440).WithMessage("Duration cannot exceed 1440 minutes (24 hours) per row.");

                    entry.RuleFor(e => e.ProjectId);
                    entry.RuleFor(e => e.TaskDescription)
                        .MaximumLength(500).WithMessage("Task description cannot exceed 500 characters.");

                    entry.RuleFor(e => e.ProjectLeadId);

                    entry.RuleFor(e => e.Location)
                        .InclusiveBetween(0, 2).WithMessage("Location must be 0 (Office), 1 (WFH), or 2 (Meeting Room).");
                });

            // Validate total duration per day does not exceed 24 hours
            RuleFor(x => x.RequestDto.Entries)
                .Must(entries => entries == null || entries.Sum(e => e.DurationMinutes) <= 1440)
                .WithMessage("Total working duration for the day cannot exceed 24 hours (1440 minutes).");
        });

    }
}



