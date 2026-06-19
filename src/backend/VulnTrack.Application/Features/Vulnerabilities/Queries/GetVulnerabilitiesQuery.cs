using MediatR;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Application.Features.Vulnerabilities.Queries;

public sealed record VulnerabilityListItemDto(
    Guid Id,
    string Title,
    Severity Severity,
    VulnerabilityStatus Status,
    string AssetName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DueDate);

public sealed record GetVulnerabilitiesQuery(
    int PageNumber = 1,
    int PageSize = 25,
    Severity? Severity = null,
    VulnerabilityStatus? Status = null,
    Guid? AssetId = null,
    string? SearchTerm = null) : IRequest<PagedResult<VulnerabilityListItemDto>>;
