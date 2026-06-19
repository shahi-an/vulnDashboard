using FluentValidation;
using VulnTrack.Application.Features.Teams.Commands;

namespace VulnTrack.Application.Features.Teams.Validators;

internal sealed class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(1000).When(x => x.Description is not null);

        RuleFor(x => x.TeamLeadEmail)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.TeamLeadEmail))
            .MaximumLength(254).When(x => x.TeamLeadEmail is not null);
    }
}
