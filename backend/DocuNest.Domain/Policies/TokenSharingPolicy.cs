using DocuNest.Domain.Entities;

namespace DocuNest.Domain.Policies;

public static class SessionPolicy
{
    public static readonly TimeSpan AccessLifetime = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan IdleLifetime = TimeSpan.FromDays(7);
    public static readonly TimeSpan AbsoluteLifetime = TimeSpan.FromDays(30);
    public static readonly TimeSpan StepUpLifetime = TimeSpan.FromMinutes(10);

    public static bool IsActive(RefreshSession session, UserAccount user, DateTimeOffset now) =>
        session.UserId == user.Id && !user.Disabled && session.RevokedAt is null
        && session.IdleExpiresAt > now && session.AbsoluteExpiresAt > now
        && session.SecurityStamp == user.SecurityStamp;

    public static bool CanRotate(RefreshSession session, UserAccount user, DateTimeOffset now) =>
        session.ConsumedAt is null && IsActive(session, user, now);

    public static bool IsReplay(RefreshSession session) => session.ConsumedAt is not null;
    public static DateTimeOffset NextIdleExpiry(DateTimeOffset now, DateTimeOffset absoluteExpiry) =>
        now + IdleLifetime < absoluteExpiry ? now + IdleLifetime : absoluteExpiry;

    public static bool HasRecentMfa(RefreshSession session, DateTimeOffset now) =>
        session.MfaVerifiedAt is { } verified && verified <= now && now - verified <= StepUpLifetime;
}

public static class SharingPolicy
{
    public static readonly TimeSpan DefaultLifetime = TimeSpan.FromHours(24);
    public static readonly TimeSpan MaximumLifetime = TimeSpan.FromDays(7);
    public static bool IsActive(ShareLink link, Document document, DateTimeOffset now) =>
        link.WorkspaceId == document.WorkspaceId && link.DocumentId == document.Id && link.RevokedAt is null
        && link.ExpiresAt > now && document.TrashedAt is null && document.PurgeRequestedAt is null;

    public static bool CanStartDownload(ShareLink link, Document document, DateTimeOffset now) =>
        link.AllowDownload && IsActive(link, document, now)
        && (link.MaximumDownloads is null || link.DownloadsStarted < link.MaximumDownloads.Value);

    public static bool ValidLifetime(DateTimeOffset now, DateTimeOffset expiresAt) =>
        expiresAt > now && expiresAt - now <= MaximumLifetime;
}
