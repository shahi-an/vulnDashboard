using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Assets.Commands;

public sealed record DeleteAssetCommand(Guid Id) : IRequest<Result>;

internal sealed class DeleteAssetCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteAssetCommand, Result>
{
    public async Task<Result> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await dbContext.Assets.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Asset), request.Id);

        asset.MarkDeleted(currentUser.UserName);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
