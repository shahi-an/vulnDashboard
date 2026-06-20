using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Infrastructure.Settings;

namespace VulnTrack.Infrastructure.Services.Azure;

internal sealed class BlobStorageService(
    BlobServiceClient blobServiceClient,
    IOptions<AzureStorageSettings> options)
    : IBlobStorageService
{
    private readonly string _containerName = options.Value.AttachmentsContainer;

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var container = blobServiceClient.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobName = $"{Guid.NewGuid():N}/{fileName}";
        var client = container.GetBlobClient(blobName);

        await client.UploadAsync(
            content,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        return client.Uri.AbsoluteUri;
    }

    public async Task<Stream> DownloadAsync(string blobUri, CancellationToken cancellationToken = default)
    {
        var client = GetAuthenticatedClient(blobUri);
        var response = await client.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value.Content;
    }

    public async Task DeleteAsync(string blobUri, CancellationToken cancellationToken = default)
    {
        var client = GetAuthenticatedClient(blobUri);
        await client.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<Uri> GenerateSasUriAsync(
        string blobUri,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var parsed = new BlobUriBuilder(new Uri(blobUri));
        var now = DateTimeOffset.UtcNow;
        var expiresOn = now.Add(expiry);

        // Managed Identity cannot sign SAS with a shared account key.
        // Use a User Delegation Key obtained from Entra ID instead.
        var delegationKey = await blobServiceClient.GetUserDelegationKeyAsync(
            startsOn: now.AddMinutes(-5),   // small clock-skew tolerance
            expiresOn: expiresOn,
            cancellationToken: cancellationToken);

        var sasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, expiresOn)
        {
            BlobContainerName = parsed.BlobContainerName,
            BlobName = parsed.BlobName,
            Resource = "b",
            StartsOn = now.AddMinutes(-5)
        };

        parsed.Sas = sasBuilder.ToSasQueryParameters(delegationKey, blobServiceClient.AccountName);
        return parsed.ToUri();
    }

    // Constructs an authenticated BlobClient using the injected BlobServiceClient
    // (which carries DefaultAzureCredential). Never create BlobClient(uri) directly —
    // that produces an anonymous client that cannot reach a private container.
    private BlobClient GetAuthenticatedClient(string blobUri)
    {
        var parsed = new BlobUriBuilder(new Uri(blobUri));
        return blobServiceClient
            .GetBlobContainerClient(parsed.BlobContainerName)
            .GetBlobClient(parsed.BlobName);
    }
}
