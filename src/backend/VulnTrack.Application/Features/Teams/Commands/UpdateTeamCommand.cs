using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Teams.Commands;

public sealed record UpdateTeamCommand(
    Guid Id,
    string Name,
    string? Description,
    string? TeamLeadEmail) : IRequest<Result>;

internal sealed class UpdateTeamCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateTeamCommand, Result>
{
    public async Task<Result> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await dbContext.Teams.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Team), request.Id);

        team.Update(request.Name, request.Description, request.TeamLeadEmail, currentUser.UserId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
