using FluentValidation;
using VulnTrack.Application.Features.Vulnerabilities.Commands;

namespace VulnTrack.Application.Features.Vulnerabilities.Validators;

internal sealed class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain",
        "text/csv",
        "image/png",
        "image/jpeg",
        "image/gif",
        "application/zip",
        "application/xml",
        "text/xml"
    ];

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    public UploadAttachmentCommandValidator()
    {
        RuleFor(x => x.VulnerabilityId).NotEmpty();

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(260);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage($"Content type must be one of: {string.Join(", ", AllowedContentTypes)}.");

        RuleFor(x => x.Content)
            .NotNull()
            .Must(s => s.Length <= MaxFileSizeBytes)
            .WithMessage($"File size must not exceed {MaxFileSizeBytes / 1024 / 1024} MB.");
    }
}
