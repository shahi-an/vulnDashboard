using MediatR;
using VulnTrack.Application.Common.Models;

namespace VulnTrack.Application.Features.Vulnerabilities.Commands;

public sealed record DeleteAttachmentCommand(Guid AttachmentId) : IRequest<Result>;
