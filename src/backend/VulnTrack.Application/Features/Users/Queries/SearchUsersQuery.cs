using MediatR;
using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Application.Features.Users.Queries;

public sealed record UserSearchResultDto(
    string Id,
    string? DisplayName,
    string? Email,
    string? JobTitle);

public sealed record SearchUsersQuery(string Query) : IRequest<IReadOnlyList<UserSearchResultDto>>;

internal sealed class SearchUsersQueryHandler(IGraphService graphService)
    : IRequestHandler<SearchUsersQuery, IReadOnlyList<UserSearchResultDto>>
{
    public async Task<IReadOnlyList<UserSearchResultDto>> Handle(
        SearchUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await graphService.SearchUsersAsync(request.Query, cancellationToken);
        return users
            .Select(u => new UserSearchResultDto(u.Id, u.DisplayName, u.Mail, u.JobTitle))
            .ToList();
    }
}
