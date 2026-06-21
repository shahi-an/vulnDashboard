using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Sources.Commands;

public sealed record ToggleSourceActiveCommand(Guid Id, bool Activate) : IRequest<Result>;

internal sealed class ToggleSourceActiveCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<ToggleSourceActiveCommand, Result>
{
    public async Task<Result> Handle(ToggleSourceActiveCommand request, CancellationToken cancellationToken)
    {
        var source = await dbContext.VulnerabilitySources.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(VulnerabilitySource), request.Id);

        if (request.Activate)
            source.Activate(currentUser.UserName);
        else
            source.Deactivate(currentUser.UserName);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
