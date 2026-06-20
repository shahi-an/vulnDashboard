using MediatR;
using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Assets.Queries;

public sealed record GetAssetByIdQuery(Guid Id) : IRequest<AssetDto>;

internal sealed class GetAssetByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAssetByIdQuery, AssetDto>
{
    public async Task<AssetDto> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var a = await dbContext.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Asset), request.Id);

        return new AssetDto(a.Id, a.Name, a.Description, a.Type.ToString(), a.Owner, a.Environment, a.CreatedAt);
    }
}
