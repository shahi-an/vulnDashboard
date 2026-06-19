namespace VulnTrack.Application.Common.Interfaces;

public interface IGraphUserDto
{
    string Id { get; }
    string? DisplayName { get; }
    string? Mail { get; }
    string? JobTitle { get; }
}

public interface IGraphService
{
    Task<IGraphUserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IGraphUserDto>> SearchUsersAsync(string query, CancellationToken cancellationToken = default);
}
