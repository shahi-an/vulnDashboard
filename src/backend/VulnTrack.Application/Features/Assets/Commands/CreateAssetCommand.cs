using MediatR;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Application.Features.Assets.Commands;

public sealed record CreateAssetCommand(
    string Name,
    AssetType Type,
    string? Description,
    string? Owner,
    string? Environment) : IRequest<Result<Guid>>;

internal sealed class CreateAssetCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateAssetCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = Asset.Create(
            request.Name,
            request.Type,
            currentUser.UserId,
            request.Description,
            request.Owner,
            request.Environment);

        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(asset.Id);
    }
}
