using DocuNest.Domain.Enums;

namespace DocuNest.Domain.Entities;

public sealed class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public long StorageQuotaBytes { get; set; } = 10L * 1024 * 1024 * 1024;
    public long UsedStorageBytes { get; set; }
    public long ReservedStorageBytes { get; set; }
    public int RetentionDays { get; set; } = 30;
    public DateTimeOffset CreatedAt { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class Membership
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public WorkspaceRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class Invitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid InviterId { get; set; }
    public string NormalizedEmail { get; set; } = "";
    public WorkspaceRole Role { get; set; }
    public string TokenHash { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class UploadReservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public Guid? DocumentId { get; set; }
    public long ReservedBytes { get; set; }
    public long ReceivedBytes { get; set; }
    public string StagingKey { get; set; } = "";
    public ReservationStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public long Version { get; set; } = 1;
}
