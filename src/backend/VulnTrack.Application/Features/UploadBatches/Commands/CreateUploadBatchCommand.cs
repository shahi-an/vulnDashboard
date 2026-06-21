using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.UploadBatches.Commands;

public sealed record CreateUploadBatchCommand(
    Guid SourceId,
    string OriginalFileName,
    Stream FileContent,
    string ContentType) : IRequest<Result<Guid>>;

internal sealed class CreateUploadBatchCommandHandler(
    IApplicationDbContext dbContext,
    IBlobStorageService blobStorage,
    IServiceBusPublisher serviceBus,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateUploadBatchCommand, Result<Guid>>
{
    private const string VulnerabilityEventsQueue = "vulnerability-events";

    public async Task<Result<Guid>> Handle(CreateUploadBatchCommand request, CancellationToken cancellationToken)
    {
        _ = await dbContext.VulnerabilitySources.FindAsync([request.SourceId], cancellationToken)
            ?? throw new NotFoundException(nameof(VulnerabilitySource), request.SourceId);

        var rawUri = await blobStorage.UploadAsync(
            request.FileContent, request.OriginalFileName, request.ContentType, cancellationToken);

        var batch = UploadBatch.Create(request.SourceId, request.OriginalFileName, currentUser.UserName, rawUri);

        dbContext.UploadBatches.Add(batch);
        await dbContext.SaveChangesAsync(cancellationToken);

        await serviceBus.PublishAsync(
            VulnerabilityEventsQueue,
            new { BatchId = batch.Id, BlobUri = rawUri, SourceId = request.SourceId },
            cancellationToken);

        return Result<Guid>.Success(batch.Id);
    }
}
