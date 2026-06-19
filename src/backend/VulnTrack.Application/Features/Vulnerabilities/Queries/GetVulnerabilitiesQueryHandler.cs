using MediatR;
using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;

namespace VulnTrack.Application.Features.Vulnerabilities.Queries;

internal sealed class GetVulnerabilitiesQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetVulnerabilitiesQuery, PagedResult<VulnerabilityListItemDto>>
{
    public async Task<PagedResult<VulnerabilityListItemDto>> Handle(
        GetVulnerabilitiesQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Vulnerabilities
            .AsNoTracking()
            .Where(v => !v.IsDeleted);

        if (request.Severity.HasValue)
            query = query.Where(v => v.Severity == request.Severity.Value);

        if (request.Status.HasValue)
            query = query.Where(v => v.Status == request.Status.Value);

        if (request.AssetId.HasValue)
            query = query.Where(v => v.AssetId == request.AssetId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(v =>
                v.Title.Contains(request.SearchTerm) ||
                (v.CveId != null && v.CveId.Contains(request.SearchTerm)));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.Severity)
            .ThenByDescending(v => v.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new VulnerabilityListItemDto(
                v.Id,
                v.Title,
                v.Severity,
                v.Status,
                v.Asset.Name,
                v.CreatedAt,
                v.DueDate))
            .ToListAsync(cancellationToken);

        return new PagedResult<VulnerabilityListItemDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
