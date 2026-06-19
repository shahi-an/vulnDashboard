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
            .Include(v => v.Source)
            .Include(v => v.Team);

        var filtered = query.AsQueryable();

        if (request.Severity.HasValue)
            filtered = filtered.Where(v => v.Severity == request.Severity.Value);

        if (request.Status.HasValue)
            filtered = filtered.Where(v => v.Status == request.Status.Value);

        if (request.VulnerabilityType.HasValue)
            filtered = filtered.Where(v => v.VulnerabilityType == request.VulnerabilityType.Value);

        if (request.TeamId.HasValue)
            filtered = filtered.Where(v => v.TeamId == request.TeamId.Value);

        if (request.SourceId.HasValue)
            filtered = filtered.Where(v => v.SourceId == request.SourceId.Value);

        if (!string.IsNullOrWhiteSpace(request.AssignedToEmail))
            filtered = filtered.Where(v => v.AssignedToEmail == request.AssignedToEmail.ToLowerInvariant());

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            filtered = filtered.Where(v =>
                v.VulnerabilityNumber.Contains(term) ||
                v.ServerName.Contains(term) ||
                v.ServerIp.Contains(term) ||
                v.Description.Contains(term) ||
                (v.CveId != null && v.CveId.Contains(term)));
        }

        var totalCount = await filtered.CountAsync(cancellationToken);

        var items = await filtered
            .OrderByDescending(v => v.Severity)
            .ThenByDescending(v => v.LastUpdated)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new VulnerabilityListItemDto(
                v.Id,
                v.VulnerabilityNumber,
                v.ServerName,
                v.ServerIp,
                v.VulnerabilityType,
                v.Severity,
                v.Status,
                v.Priority,
                v.AssignedToEmail,
                v.Team != null ? v.Team.Name : null,
                v.Source.Name,
                v.LastUpdated,
                v.FollowUpDue,
                v.Ecd))
            .ToListAsync(cancellationToken);

        return new PagedResult<VulnerabilityListItemDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
