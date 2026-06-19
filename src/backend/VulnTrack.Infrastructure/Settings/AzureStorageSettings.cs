namespace VulnTrack.Infrastructure.Settings;

public sealed class AzureStorageSettings
{
    public const string SectionName = "AzureStorage";
    public string AccountName { get; init; } = string.Empty;
    public string AttachmentsContainer { get; init; } = "attachments";
}
