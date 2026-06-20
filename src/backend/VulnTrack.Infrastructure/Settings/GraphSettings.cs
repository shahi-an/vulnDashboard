namespace VulnTrack.Infrastructure.Settings;

public sealed class GraphSettings
{
    public const string SectionName = "Graph";
    public string SenderEmail { get; init; } = string.Empty;
}
