using MediatR;
using VulnTrack.Application.Common.Models;

namespace VulnTrack.Application.Features.Vulnerabilities.Commands;

public sealed record UploadAttachmentCommand(
    Guid VulnerabilityId,
    string FileName,
    string ContentType,
    Stream Content,
    Guid? UploadBatchId = null) : IRequest<Result<Guid>>;
