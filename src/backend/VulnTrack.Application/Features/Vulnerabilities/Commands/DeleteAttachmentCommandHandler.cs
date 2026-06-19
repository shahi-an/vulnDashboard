using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Vulnerabilities.Commands;

internal sealed class DeleteAttachmentCommandHandler(
    IApplicationDbContext dbContext,
    IBlobStorageService blobStorage)
    : IRequestHandler<DeleteAttachmentCommand, Result>
{
    public async Task<Result> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await dbContext.Attachments.FindAsync([request.AttachmentId], cancellationToken)
            ?? throw new NotFoundException(nameof(Attachment), request.AttachmentId);

        await blobStorage.DeleteAsync(attachment.BlobUri, cancellationToken);

        dbContext.Attachments.Remove(attachment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
