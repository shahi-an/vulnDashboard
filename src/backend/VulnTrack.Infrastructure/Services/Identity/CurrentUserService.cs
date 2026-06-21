using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Infrastructure.Services.Identity;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal?.FindFirstValue("oid")
        ?? string.Empty;

    public string? UserEmail => Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue("preferred_username");

    public string? DisplayName => Principal?.FindFirstValue("name");

    public string UserName => DisplayName ?? UserEmail ?? UserId;

    public IReadOnlyList<string> Roles => Principal?
        .FindAll(ClaimTypes.Role)
        .Select(c => c.Value)
        .ToList() ?? [];

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
