using MediatR;
using Microsoft.EntityFrameworkCore;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Teams.Queries;

public sealed record GetTeamByIdQuery(Guid Id) : IRequest<TeamDto>;

internal sealed class GetTeamByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTeamByIdQuery, TeamDto>
{
    public async Task<TeamDto> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Teams
            .AsNoTracking()
            .Where(t => t.Id == request.Id)
            .Select(t => new TeamDto(
                t.Id,
                t.Name,
                t.Description,
                t.TeamLeadEmail,
                t.Vulnerabilities.Count(v => !v.IsDeleted)))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Team), request.Id);
    }
}
