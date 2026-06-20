using VulnTrack.Application.Common.Interfaces;

namespace VulnTrack.Infrastructure.Services.Dev;

internal sealed class LocalBlobStorageService : IBlobStorageService
{
    private static readonly string StorePath =
        Path.Combine(Path.GetTempPath(), "vulntrack-dev-blobs");

    public async Task<string> UploadAsync(
        Stream content, string fileName, string contentType,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(StorePath);
        var blobName = $"{Guid.NewGuid():N}-{Path.GetFileName(fileName)}";
        await using var file = File.Create(Path.Combine(StorePath, blobName));
        await content.CopyToAsync(file, cancellationToken);
        return $"local://{blobName}";
    }

    public Task<Stream> DownloadAsync(string blobUri, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(StorePath, blobUri.Replace("local://", string.Empty));
        return Task.FromResult<Stream>(File.OpenRead(path));
    }

    public Task DeleteAsync(string blobUri, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(StorePath, blobUri.Replace("local://", string.Empty));
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<Uri> GenerateSasUriAsync(string blobUri, TimeSpan expiry,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new Uri($"https://localhost:7001/dev-blob/{Uri.EscapeDataString(blobUri)}"));
}
