using MediatR;

namespace VulnTrack.Application.Features.Vulnerabilities.Queries;

public sealed record GetAttachmentDownloadUrlQuery(Guid AttachmentId) : IRequest<Uri>;
