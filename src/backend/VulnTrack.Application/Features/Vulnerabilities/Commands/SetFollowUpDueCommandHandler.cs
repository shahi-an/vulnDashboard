using MediatR;
using VulnTrack.Application.Common.Exceptions;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Entities;

namespace VulnTrack.Application.Features.Vulnerabilities.Commands;

internal sealed class SetFollowUpDueCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<SetFollowUpDueCommand, Result>
{
    public async Task<Result> Handle(SetFollowUpDueCommand request, CancellationToken cancellationToken)
    {
        var vulnerability = await dbContext.Vulnerabilities.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Vulnerability), request.Id);

        vulnerability.SetFollowUpDue(request.FollowUpDue, currentUser.UserId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
