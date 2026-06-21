namespace VulnTrack.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    string? UserEmail { get; }
    string? DisplayName { get; }

    /// Human-readable audit label: DisplayName → UserEmail → UserId (sub).
    string UserName { get; }

    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
}
