using MediatR;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Teams.Commands;

public sealed record CreateTeamCommand(
    string Name,
    string? Description,
    string? TeamLeadEmail) : IRequest<Result<Guid>>;

internal sealed class CreateTeamCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateTeamCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = Team.Create(request.Name, currentUser.UserId, request.Description, request.TeamLeadEmail);
        dbContext.Teams.Add(team);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(team.Id);
    }
}
