namespace VulnTrack.Infrastructure.Settings;

public sealed class ServiceBusSettings
{
    public const string SectionName = "ServiceBus";
    public string Namespace { get; init; } = string.Empty;
    public string VulnerabilityEventsQueue { get; init; } = "vulnerability-events";
    public string NotificationsQueue { get; init; } = "notifications";
}
