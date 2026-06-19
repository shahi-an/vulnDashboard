using MediatR;
using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Vulnerabilities.Commands;

internal sealed class UploadAttachmentCommandHandler(
    IApplicationDbContext dbContext,
    IBlobStorageService blobStorage,
    ICurrentUserService currentUser)
    : IRequestHandler<UploadAttachmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Vulnerabilities
            .AnyAsync(v => v.Id == request.VulnerabilityId, cancellationToken);

        if (!exists)
            throw new NotFoundException(nameof(Vulnerability), request.VulnerabilityId);

        var blobUri = await blobStorage.UploadAsync(
            request.Content, request.FileName, request.ContentType, cancellationToken);

        var attachment = Attachment.Create(
            request.VulnerabilityId,
            request.FileName,
            request.ContentType,
            blobUri,
            request.Content.Length,
            currentUser.UserId,
            request.UploadBatchId);

        dbContext.Attachments.Add(attachment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(attachment.Id);
    }
}
