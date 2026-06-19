using VulnTrack.Domain.Common;
using VulnTrack.Domain.Enums;

namespace VulnTrack.Domain.Entities;

/// <summary>
/// Infrastructure asset catalogue entry.
/// Vulnerabilities embed ServerName/ServerIp directly and reference
/// VulnerabilitySource; this entity is retained for future CMDB integration.
/// </summary>
public sealed class Asset : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AssetType Type { get; private set; }
    public string? Owner { get; private set; }
    public string? Environment { get; private set; }
    public string? Tags { get; private set; }

    private Asset() { }

    public static Asset Create(
        string name,
        AssetType type,
        string createdBy,
        string? description = null,
        string? owner = null,
        string? environment = null)
    {
        return new Asset
        {
            Name = name,
            Type = type,
            CreatedBy = createdBy,
            Description = description,
            Owner = owner,
            Environment = environment
        };
    }

    public void Update(string name, AssetType type, string? owner, string? environment, string updatedBy)
    {
        Name = name;
        Type = type;
        Owner = owner;
        Environment = environment;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
