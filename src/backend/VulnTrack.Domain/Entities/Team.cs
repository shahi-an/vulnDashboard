using VulnTrack.Domain.Common;

namespace VulnTrack.Domain.Entities;

public sealed class Team : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? TeamLeadEmail { get; private set; }

    private readonly List<Vulnerability> _vulnerabilities = [];
    public IReadOnlyCollection<Vulnerability> Vulnerabilities => _vulnerabilities.AsReadOnly();

    private Team() { }

    public static Team Create(string name, string createdBy, string? description = null, string? teamLeadEmail = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be empty.", nameof(name));

        return new Team
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            TeamLeadEmail = teamLeadEmail?.Trim().ToLowerInvariant(),
            CreatedBy = createdBy
        };
    }

    public void Update(string name, string? description, string? teamLeadEmail, string updatedBy)
    {
        Name = name.Trim();
        Description = description?.Trim();
        TeamLeadEmail = teamLeadEmail?.Trim().ToLowerInvariant();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
