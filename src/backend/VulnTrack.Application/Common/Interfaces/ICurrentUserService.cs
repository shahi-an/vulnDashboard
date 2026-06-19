namespace VulnTrack.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    string? UserEmail { get; }
    string? DisplayName { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
}
