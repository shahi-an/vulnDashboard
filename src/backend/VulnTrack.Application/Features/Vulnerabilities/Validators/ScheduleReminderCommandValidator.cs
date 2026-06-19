using FluentValidation;
using VulnTrack.Application.Features.Vulnerabilities.Commands;

namespace VulnTrack.Application.Features.Vulnerabilities.Validators;

internal sealed class ScheduleReminderCommandValidator : AbstractValidator<ScheduleReminderCommand>
{
    public ScheduleReminderCommandValidator()
    {
        RuleFor(x => x.VulnerabilityId).NotEmpty();

        RuleFor(x => x.RecipientEmail)
            .NotEmpty()
            .MaximumLength(254)
            .EmailAddress();

        RuleFor(x => x.ScheduledFor)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Reminder must be scheduled for a future date and time.");

        RuleFor(x => x.Message)
            .MaximumLength(2000).When(x => x.Message is not null);
    }
}
