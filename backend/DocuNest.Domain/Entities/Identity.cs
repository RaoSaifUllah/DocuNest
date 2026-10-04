using DocuNest.Domain.Enums;

namespace DocuNest.Domain.Entities;

public sealed class UserAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    public string NormalizedEmail { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
    public bool IsSystemAdmin { get; set; }
    public bool EmailVerified { get; set; }
    public bool Disabled { get; set; }
    public string? EncryptedMfaSecret { get; set; }
    public bool MfaEnabled { get; set; }
    public long? LastMfaCounter { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public int FailedLoginCount { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class RefreshSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid FamilyId { get; set; }
    public string TokenHash { get; set; } = "";
    public string CsrfHash { get; set; } = "";
    public string SecurityStamp { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public DateTimeOffset IdleExpiresAt { get; set; }
    public DateTimeOffset AbsoluteExpiresAt { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
    public Guid? ReplacedBySessionId { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
    public DateTimeOffset? MfaVerifiedAt { get; set; }
    public string ClientLabel { get; set; } = "Browser";
    public long Version { get; set; } = 1;
}

public sealed class CredentialToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string TokenHash { get; set; } = "";
    public CredentialTokenKind Kind { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class MfaRecoveryCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = "";
    public DateTimeOffset? ConsumedAt { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class ConfigurationSecret
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Ciphertext { get; set; } = "";
    public string KeyVersion { get; set; } = "";
    public DateTimeOffset UpdatedAt { get; set; }
    public long Version { get; set; } = 1;
}
