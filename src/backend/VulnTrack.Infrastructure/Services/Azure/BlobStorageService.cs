using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Infrastructure.Settings;

namespace VulnTrack.Infrastructure.Services.Azure;

internal sealed class BlobStorageService(BlobServiceClient blobServiceClient, IOptions<AzureStorageSettings> options)
    : IBlobStorageService
{
    private readonly string _containerName = options.Value.AttachmentsContainer;

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var container = blobServiceClient.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobName = $"{Guid.NewGuid():N}/{fileName}";
        var client = container.GetBlobClient(blobName);

        await client.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        return client.Uri.AbsoluteUri;
    }

    public async Task<Stream> DownloadAsync(string blobUri, CancellationToken cancellationToken = default)
    {
        var client = new BlobClient(new Uri(blobUri));
        var response = await client.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteAsync(string blobUri, CancellationToken cancellationToken = default)
    {
        var client = new BlobClient(new Uri(blobUri));
        await client.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public Task<Uri> GenerateSasUriAsync(string blobUri, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var client = new BlobClient(new Uri(blobUri));
        var sasUri = client.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiry));
        return Task.FromResult(sasUri);
    }
}
