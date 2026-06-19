using MediatR;
using VulnTrack.Application.Common.Models;

namespace VulnTrack.Application.Features.Vulnerabilities.Commands;

public sealed record SetFollowUpDueCommand(Guid Id, DateTimeOffset FollowUpDue) : IRequest<Result>;
