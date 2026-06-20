using FluentValidation;
using VulnTrack.Application.Features.Assets.Commands;

namespace VulnTrack.Application.Features.Assets.Validators;

internal sealed class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.Owner).MaximumLength(200).When(x => x.Owner is not null);
        RuleFor(x => x.Environment).MaximumLength(50).When(x => x.Environment is not null);
    }
}
