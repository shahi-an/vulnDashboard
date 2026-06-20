using MediatR;
using VulnTrack.Application.Common.Models;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Application.Features.Vulnerabilities.Queries;

public sealed record VulnerabilityListItemDto(
    Guid Id,
    string VulnerabilityNumber,
    string ServerName,
    string ServerIp,
    VulnerabilityType VulnerabilityType,
    Severity Severity,
    VulnerabilityStatus Status,
    RemediationPriority Priority,
    string? AssignedToEmail,
    string? TeamName,
    string SourceName,
    DateTimeOffset LastUpdated,
    DateTimeOffset? FollowUpDue,
    DateTimeOffset? Ecd);

public sealed record GetVulnerabilitiesQuery(
    int PageNumber = 1,
    int PageSize = 25,
    Severity? Severity = null,
    VulnerabilityStatus? Status = null,
    VulnerabilityType? VulnerabilityType = null,
    Guid? TeamId = null,
    Guid? SourceId = null,
    string? AssignedToEmail = null,
    string? SearchTerm = null,
    DateTimeOffset? CreatedAfter = null,
    DateTimeOffset? CreatedBefore = null,
    DateTimeOffset? FollowUpDueBefore = null) : IRequest<PagedResult<VulnerabilityListItemDto>>;
