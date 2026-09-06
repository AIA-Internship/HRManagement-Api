using HRManagement.Domain.Interfaces;
using FluentValidation;
using HRManagement.Application.Commands.Timesheet;

namespace HRManagement.Application.Validators.Timesheet;

public class SubmitTimesheetValidator : AbstractValidator<SubmitTimesheetCommand>
{
    public SubmitTimesheetValidator()
    {
        RuleFor(x => x.RequestDto.Month)
            .InclusiveBetween(1, 12).WithMessage("Bulan pengajuan tidak valid. Harus antara 1 sampai 12.");

        RuleFor(x => x.RequestDto.Year)
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(7).Year)
            .WithMessage("Tahun pengajuan tidak boleh untuk masa depan.");

        RuleFor(x => x.RequestDto)
            .Must(dto =>
            {
                var today = DateTime.UtcNow.AddHours(7);
                if (dto.Year > today.Year) return false;
                if (dto.Year == today.Year && dto.Month > today.Month) return false;
                return true;
            })
            .WithMessage(x => {
                try {
                    var monthName = new System.Globalization.CultureInfo("en-US").DateTimeFormat.GetMonthName(x.RequestDto.Month);
                    return $"Anda tidak dapat mengirim timesheet untuk periode masa depan ({monthName} {x.RequestDto.Year}).";
                } catch { return "Periode waktu tidak valid."; }
            });
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



