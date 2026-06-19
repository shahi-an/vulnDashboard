using MediatR;
using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Domain.Entities;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Application.Features.UploadBatches.Queries;

public sealed record UploadBatchDetailDto(
    Guid Id,
    string OriginalFileName,
    string? RawFileBlobUri,
    Guid SourceId,
    string SourceName,
    UploadBatchStatus Status,
    int TotalRecords,
    int ProcessedCount,
    int SuccessCount,
    int FailureCount,
    string? ErrorSummary,
    string UploadedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record GetUploadBatchByIdQuery(Guid Id) : IRequest<UploadBatchDetailDto>;

internal sealed class GetUploadBatchByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetUploadBatchByIdQuery, UploadBatchDetailDto>
{
    public async Task<UploadBatchDetailDto> Handle(
        GetUploadBatchByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.UploadBatches
            .AsNoTracking()
            .Include(b => b.Source)
            .Where(b => b.Id == request.Id)
            .Select(b => new UploadBatchDetailDto(
                b.Id,
                b.OriginalFileName,
                b.RawFileBlobUri,
                b.SourceId,
                b.Source.Name,
                b.Status,
                b.TotalRecords,
                b.ProcessedCount,
                b.SuccessCount,
                b.FailureCount,
                b.ErrorSummary,
                b.CreatedBy,
                b.CreatedAt,
                b.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(UploadBatch), request.Id);
    }
}
