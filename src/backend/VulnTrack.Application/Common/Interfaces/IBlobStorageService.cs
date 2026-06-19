namespace VulnTrack.Application.Common.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string blobUri, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobUri, CancellationToken cancellationToken = default);
    Task<Uri> GenerateSasUriAsync(string blobUri, TimeSpan expiry, CancellationToken cancellationToken = default);
}
