using FluentValidation;
using VulnTrack.Application.Features.Vulnerabilities.Commands;

namespace VulnTrack.Application.Features.Vulnerabilities.Validators;

internal sealed class SetFollowUpDueCommandValidator : AbstractValidator<SetFollowUpDueCommand>
{
    public SetFollowUpDueCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.FollowUpDue)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Follow-up due date must be a future date.");
    }
}
