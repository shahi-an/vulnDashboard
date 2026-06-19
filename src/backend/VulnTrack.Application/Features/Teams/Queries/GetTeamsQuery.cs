using MediatR;
using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;

namespace VulnTrack.Application.Features.Teams.Queries;

public sealed record TeamDto(
    Guid Id,
    string Name,
    string? Description,
    string? TeamLeadEmail,
    int VulnerabilityCount);

public sealed record GetTeamsQuery(string? SearchTerm = null) : IRequest<IReadOnlyList<TeamDto>>;

internal sealed class GetTeamsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeamsQuery, IReadOnlyList<TeamDto>>
{
    public async Task<IReadOnlyList<TeamDto>> Handle(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Teams.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(t => t.Name.Contains(request.SearchTerm));

        return await query
            .OrderBy(t => t.Name)
            .Select(t => new TeamDto(
                t.Id,
                t.Name,
                t.Description,
                t.TeamLeadEmail,
                t.Vulnerabilities.Count(v => !v.IsDeleted)))
            .ToListAsync(cancellationToken);
    }
}
