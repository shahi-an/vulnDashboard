using Microsoft.Graph;
using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Infrastructure.Services.Graph;

internal sealed record GraphUserDto(string Id, string? DisplayName, string? Mail, string? JobTitle) : IGraphUserDto;

internal sealed class GraphService(GraphServiceClient graphClient) : IGraphService
{
    public async Task<IGraphUserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await graphClient.Users[userId].GetAsync(cancellationToken: cancellationToken);
        if (user is null) return null;

        return new GraphUserDto(user.Id!, user.DisplayName, user.Mail, user.JobTitle);
    }

    public async Task<IReadOnlyList<IGraphUserDto>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
    {
        var result = await graphClient.Users.GetAsync(req =>
        {
            req.QueryParameters.Filter = $"startswith(displayName,'{query}') or startswith(mail,'{query}')";
            req.QueryParameters.Top = 20;
            req.QueryParameters.Select = ["id", "displayName", "mail", "jobTitle"];
        }, cancellationToken);

        return result?.Value?
            .Select(u => (IGraphUserDto)new GraphUserDto(u.Id!, u.DisplayName, u.Mail, u.JobTitle))
            .ToList() ?? [];
    }
}
