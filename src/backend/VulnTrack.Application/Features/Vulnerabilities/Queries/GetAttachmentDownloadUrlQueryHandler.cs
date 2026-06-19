using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Vulnerabilities.Queries;

internal sealed class GetAttachmentDownloadUrlQueryHandler(
    IApplicationDbContext dbContext,
    IBlobStorageService blobStorage)
    : IRequestHandler<GetAttachmentDownloadUrlQuery, Uri>
{
    private static readonly TimeSpan SasExpiry = TimeSpan.FromMinutes(15);

    public async Task<Uri> Handle(GetAttachmentDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var attachment = await dbContext.Attachments.FindAsync([request.AttachmentId], cancellationToken)
            ?? throw new NotFoundException(nameof(Attachment), request.AttachmentId);

        return await blobStorage.GenerateSasUriAsync(attachment.BlobUri, SasExpiry, cancellationToken);
    }
}
