using MediatR;
using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Application.Features.Assets.Queries;

public sealed record AssetDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string? Owner,
    string? Environment,
    DateTimeOffset CreatedAt);

public sealed record GetAssetsQuery(string? Search = null, string? Type = null) : IRequest<IReadOnlyList<AssetDto>>;

internal sealed class GetAssetsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAssetsQuery, IReadOnlyList<AssetDto>>
{
    public async Task<IReadOnlyList<AssetDto>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Assets.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(a => a.Name.Contains(request.Search) || (a.Owner != null && a.Owner.Contains(request.Search)));

        if (!string.IsNullOrWhiteSpace(request.Type))
            query = query.Where(a => a.Type.ToString() == request.Type);

        return await query
            .OrderBy(a => a.Name)
            .Select(a => new AssetDto(a.Id, a.Name, a.Description, a.Type.ToString(), a.Owner, a.Environment, a.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
