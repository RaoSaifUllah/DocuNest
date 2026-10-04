using DocuNest.Domain.Enums;

namespace DocuNest.Domain.Entities;

public sealed class Reminder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public string TimeZoneId { get; set; } = "UTC";
    public DateOnly LocalDate { get; set; }
    public TimeOnly LocalTime { get; set; } = new(9, 0);
    public int? RelativeDays { get; set; }
    public DateTimeOffset DueAt { get; set; }
    public long Generation { get; set; } = 1;
    public ReminderStatus Status { get; set; }
    public ReminderKind Kind { get; set; }
    public NotificationChannel Channel { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public Guid ReminderId { get; set; }
    public long Generation { get; set; }
    public NotificationChannel Channel { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class ShareLink
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid CreatorId { get; set; }
    public string TokenHash { get; set; } = "";
    public string? PasswordHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public bool AllowDownload { get; set; }
    public int? MaximumDownloads { get; set; }
    public int DownloadsStarted { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class ShareSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid ShareLinkId { get; set; }
    public string TokenHash { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? DownloadStartedAt { get; set; }
    public long Version { get; set; } = 1;
}
