using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Application.Features.Assets.Commands;

public sealed record UpdateAssetCommand(
    Guid Id,
    string Name,
    AssetType Type,
    string? Description,
    string? Owner,
    string? Environment) : IRequest<Result>;

internal sealed class UpdateAssetCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateAssetCommand, Result>
{
    public async Task<Result> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Assets.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Asset), request.Id);

        asset.Update(request.Name, request.Type, request.Description, request.Owner, request.Environment, currentUser.UserName);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
