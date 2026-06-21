using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Teams.Commands;

public sealed record DeleteTeamCommand(Guid Id) : IRequest<Result>;

internal sealed class DeleteTeamCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteTeamCommand, Result>
{
    public async Task<Result> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await dbContext.Teams.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Team), request.Id);

        team.MarkDeleted(currentUser.UserName);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
