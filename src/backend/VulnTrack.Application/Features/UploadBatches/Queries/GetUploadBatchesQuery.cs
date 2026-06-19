using MediatR;
using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Application.Features.UploadBatches.Queries;

public sealed record UploadBatchListDto(
    Guid Id,
    string OriginalFileName,
    string SourceName,
    UploadBatchStatus Status,
    int TotalRecords,
    int SuccessCount,
    int FailureCount,
    string UploadedBy,
    DateTimeOffset CreatedAt);

public sealed record GetUploadBatchesQuery(
    int PageNumber = 1,
    int PageSize = 25,
    Guid? SourceId = null,
    UploadBatchStatus? Status = null) : IRequest<PagedResult<UploadBatchListDto>>;

internal sealed class GetUploadBatchesQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetUploadBatchesQuery, PagedResult<UploadBatchListDto>>
{
    public async Task<PagedResult<UploadBatchListDto>> Handle(
        GetUploadBatchesQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.UploadBatches
            .AsNoTracking()
            .Include(b => b.Source);

        var filtered = query.AsQueryable();

        if (request.SourceId.HasValue)
            filtered = filtered.Where(b => b.SourceId == request.SourceId.Value);

        if (request.Status.HasValue)
            filtered = filtered.Where(b => b.Status == request.Status.Value);

        var totalCount = await filtered.CountAsync(cancellationToken);

        var items = await filtered
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new UploadBatchListDto(
                b.Id,
                b.OriginalFileName,
                b.Source.Name,
                b.Status,
                b.TotalRecords,
                b.SuccessCount,
                b.FailureCount,
                b.CreatedBy,
                b.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<UploadBatchListDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
