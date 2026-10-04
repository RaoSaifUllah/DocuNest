using DocuNest.Application.Common;
using DocuNest.Application.Policies;
using DocuNest.Domain.Entities;
using DocuNest.Domain.Enums;
using DocuNest.Domain.Policies;
using Xunit;

namespace DocuNest.UnitTests;

public sealed class PolicyTests
{
    private static readonly Guid WorkspaceId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid UserId = Guid.Parse("20000000-0000-0000-0000-000000000001");

    [Theory]
    [InlineData(WorkspaceRole.Owner, DocumentVisibility.Restricted, false, true, true, true, true)]
    [InlineData(WorkspaceRole.Member, DocumentVisibility.Restricted, false, false, false, false, false)]
    [InlineData(WorkspaceRole.Member, DocumentVisibility.Workspace, false, true, true, false, false)]
    [InlineData(WorkspaceRole.Member, DocumentVisibility.Restricted, true, true, true, true, true)]
    [InlineData(WorkspaceRole.ReadOnly, DocumentVisibility.Workspace, true, true, false, false, false)]
    [InlineData(WorkspaceRole.ReadOnly, DocumentVisibility.Restricted, false, false, false, false, false)]
    public void Permission_matrix(WorkspaceRole role, DocumentVisibility visibility, bool grantExists,
        bool view, bool edit, bool trash, bool share)
    {
        var document = Doc(); document.Visibility = visibility;
        var member = new Membership { WorkspaceId = WorkspaceId, UserId = UserId, Role = role };
        var grant = grantExists ? Grant(document) : null;
        Assert.Equal(view, DocumentPermissions.CanView(UserId, member, document, grant));
        Assert.Equal(edit, DocumentPermissions.CanEdit(UserId, member, document, grant));
        Assert.Equal(trash, DocumentPermissions.CanTrash(UserId, member, document, grant));
        Assert.Equal(share, DocumentPermissions.CanShare(UserId, member, document, grant));
        Assert.Equal(share, DocumentPermissions.CanExport(UserId, member, document, grant));
        Assert.Equal(view, DocumentPermissions.CanCreatePersonalReminder(UserId, member, document, grant));
    }

    [Fact]
    public void Foreign_membership_and_grants_do_not_authorize()
    {
        var document = Doc(); document.Visibility = DocumentVisibility.Restricted;
        var member = new Membership { WorkspaceId = Guid.NewGuid(), UserId = UserId, Role = WorkspaceRole.Owner };
        Assert.False(DocumentPermissions.CanView(UserId, member, document, Grant(document)));
        member.WorkspaceId = WorkspaceId; member.Role = WorkspaceRole.Member;
        var grant = Grant(document); grant.WorkspaceId = Guid.NewGuid();
        Assert.False(DocumentPermissions.CanView(UserId, member, document, grant));
        Assert.False(DocumentPermissions.CanView(UserId, null, document, Grant(document)));
    }

    [Fact]
    public void Restricted_uploader_can_edit_but_needs_explicit_delete_share_export_grants()
    {
        var document = Doc(); document.Visibility = DocumentVisibility.Restricted; document.UploaderId = UserId;
        var member = new Membership { WorkspaceId = WorkspaceId, UserId = UserId, Role = WorkspaceRole.Member };
        Assert.True(DocumentPermissions.CanEdit(UserId, member, document, null));
        Assert.False(DocumentPermissions.CanTrash(UserId, member, document, null));
        Assert.False(DocumentPermissions.CanShare(UserId, member, document, null));
        Assert.False(DocumentPermissions.CanExport(UserId, member, document, null));
    }

    [Fact]
    public void Downloads_require_clean_promoted_current_blobs_and_live_document()
    {
        var document = Doc();
        var member = new Membership { WorkspaceId = WorkspaceId, UserId = UserId, Role = WorkspaceRole.Owner };
        var blob = new Blob { WorkspaceId = WorkspaceId, DocumentId = document.Id, ScanStatus = ScanStatus.Pending, Kind = BlobKind.Original };
        Assert.False(DocumentPermissions.CanDownload(UserId, member, document, null, blob));
        blob.ScanStatus = ScanStatus.Clean; blob.PromotedAt = DateTimeOffset.UtcNow;
        Assert.True(DocumentPermissions.CanDownload(UserId, member, document, null, blob));
        blob.Kind = BlobKind.Preview; blob.Generation = document.ProcessingGeneration - 1;
        Assert.False(DocumentPermissions.CanDownload(UserId, member, document, null, blob));
        blob.Generation = document.ProcessingGeneration; document.TrashedAt = DateTimeOffset.UtcNow;
        Assert.False(DocumentPermissions.CanDownload(UserId, member, document, null, blob));
    }

    [Fact]
    public void Last_owner_cannot_leave_or_be_demoted()
    {
        Assert.False(WorkspacePermissions.CanChangeRole(WorkspaceRole.Owner, null, 1));
        Assert.False(WorkspacePermissions.CanChangeRole(WorkspaceRole.Owner, WorkspaceRole.Member, 1));
        Assert.True(WorkspacePermissions.CanChangeRole(WorkspaceRole.Owner, WorkspaceRole.Owner, 1));
        Assert.True(WorkspacePermissions.CanChangeRole(WorkspaceRole.Owner, null, 2));
    }

    [Fact]
    public void Reprocessing_does_not_override_confirmed_fields_and_rejects_stale_suggestions()
    {
        var document = Doc();
        MetadataPolicy.Edit(document, new Dictionary<string, string?> { ["title"] = "User title", ["vendor"] = "Edited vendor" }, 1, DateTimeOffset.UtcNow);
        Assert.False(MetadataPolicy.CanSuggest(document, "title", 1));
        Assert.True(MetadataPolicy.CanSuggest(document, "amount", 1));
        ProcessingPolicy.InvalidateGeneration(document, DateTimeOffset.UtcNow);
        Assert.Equal("User title", document.Title);
        Assert.False(MetadataPolicy.AcceptSuggestion(document, new ExtractionSuggestion { WorkspaceId = WorkspaceId, DocumentId = document.Id, Field = "title", Value = "Stale", Generation = 1 }, 1, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Invalid_metadata_patch_cannot_partially_mutate()
    {
        var document = Doc();
        Assert.Throws<UseCaseException>(() => MetadataPolicy.Edit(document, new Dictionary<string, string?> { ["title"] = "Changed", ["issueDate"] = "not-a-date" }, 1, DateTimeOffset.UtcNow));
        Assert.Equal("Synthetic invoice", document.Title);
        Assert.Equal("[]", document.ConfirmedFieldsJson);
        Assert.Equal(428, Assert.Throws<UseCaseException>(() => CommandPreconditions.RequireVersion(null, 1)).StatusCode);
        Assert.Equal(409, Assert.Throws<UseCaseException>(() => CommandPreconditions.RequireVersion(2, 1)).StatusCode);
    }

    [Fact]
    public void Quota_arithmetic_cannot_overflow_or_ignore_concurrent_reservations()
    {
        var workspace = new Workspace { StorageQuotaBytes = 100, UsedStorageBytes = 40, ReservedStorageBytes = 30 };
        Assert.True(QuotaPolicy.CanReserve(workspace, 30));
        Assert.False(QuotaPolicy.CanReserve(workspace, 31));
        Assert.False(QuotaPolicy.CanReserve(workspace, long.MaxValue));
        Assert.False(QuotaPolicy.CanReserve(workspace, -1));
    }

    [Fact]
    public void Stale_generation_and_lease_fences_cannot_publish()
    {
        var now = DateTimeOffset.UtcNow;
        var document = Doc(); document.ProcessingStatus = DocumentProcessingStatus.Processing;
        Assert.True(ProcessingPolicy.CanPublish(document, 1));
        ProcessingPolicy.InvalidateGeneration(document, now);
        Assert.False(ProcessingPolicy.CanPublish(document, 1));
        var job = new DurableJob { Status = JobStatus.Running, LeaseOwner = "worker-a", Fence = 2, LeaseExpiresAt = now.AddMinutes(1) };
        Assert.False(ProcessingPolicy.LeaseIsCurrent(job, "worker-a", 1, now));
        Assert.False(ProcessingPolicy.LeaseIsCurrent(job, "worker-b", 2, now));
        Assert.True(ProcessingPolicy.LeaseIsCurrent(job, "worker-a", 2, now));
        Assert.False(ProcessingPolicy.LeaseIsCurrent(job, "worker-a", 2, now.AddMinutes(1)));
    }

    [Fact]
    public void DST_gap_moves_forward_and_overlap_uses_first_instant()
    {
        Assert.Equal(new DateTimeOffset(2026, 3, 8, 7, 0, 0, TimeSpan.Zero), ReminderCalendar.Resolve(new DateOnly(2026, 3, 8), new TimeOnly(2, 30), "America/New_York"));
        Assert.Equal(new DateTimeOffset(2026, 11, 1, 5, 30, 0, TimeSpan.Zero), ReminderCalendar.Resolve(new DateOnly(2026, 11, 1), new TimeOnly(1, 30), "America/New_York"));
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 4, 0, 0, TimeSpan.Zero), ReminderCalendar.Resolve(new DateOnly(2026, 10, 4), new TimeOnly(9, 0), "Asia/Karachi"));
    }

    [Fact]
    public void Session_replay_expiration_and_stamp_changes_fail_closed()
    {
        var now = DateTimeOffset.UtcNow;
        var user = new UserAccount { Id = UserId, SecurityStamp = "stamp" };
        var session = new RefreshSession { UserId = UserId, SecurityStamp = "stamp", IdleExpiresAt = now.AddDays(7), AbsoluteExpiresAt = now.AddDays(30) };
        Assert.True(SessionPolicy.CanRotate(session, user, now));
        session.ConsumedAt = now;
        Assert.True(SessionPolicy.IsReplay(session)); Assert.False(SessionPolicy.CanRotate(session, user, now));
        session.ConsumedAt = null; user.SecurityStamp = "new";
        Assert.False(SessionPolicy.IsActive(session, user, now));
        user.SecurityStamp = "stamp";
        Assert.False(SessionPolicy.IsActive(session, user, now.AddDays(7)));
        Assert.Equal(session.AbsoluteExpiresAt, SessionPolicy.NextIdleExpiry(now.AddDays(29), session.AbsoluteExpiresAt));
    }

    [Theory]
    [InlineData("../outside.pdf")]
    [InlineData("/etc/file.pdf")]
    [InlineData("C:/file.pdf")]
    [InlineData("folder\\file.pdf")]
    [InlineData("folder//file.pdf")]
    [InlineData("CON.pdf")]
    [InlineData("folder/../file.pdf")]
    public void Unsafe_import_paths_are_rejected(string path) => Assert.Throws<UseCaseException>(() => ImportPolicy.ValidateRelativePath(path, new()));

    [Fact]
    public void Import_rejects_symlinks_compression_bombs_and_total_overflow()
    {
        var limits = new ImportLimits();
        Assert.Equal("invoices/2026/synthetic.pdf", ImportPolicy.ValidateRelativePath("invoices/2026/synthetic.pdf", limits));
        Assert.Throws<UseCaseException>(() => ImportPolicy.ValidateRelativePath("safe.pdf", limits, true));
        Assert.Throws<UseCaseException>(() => ImportPolicy.ValidateEntry(10_000, 1, 1, 0, limits));
        Assert.Throws<UseCaseException>(() => ImportPolicy.ValidateEntry(20, 20, 1, long.MaxValue, limits));
        Assert.Throws<UseCaseException>(() => ImportPolicy.ValidateEntry(1, 1, 1001, 0, limits));
    }

    [Fact]
    public void Share_budget_and_revocation_apply_before_new_transfer()
    {
        var now = DateTimeOffset.UtcNow; var document = Doc();
        var link = new ShareLink { WorkspaceId = WorkspaceId, DocumentId = document.Id, AllowDownload = true, MaximumDownloads = 1, ExpiresAt = now.AddHours(24) };
        Assert.True(SharingPolicy.CanStartDownload(link, document, now));
        link.DownloadsStarted = 1; Assert.False(SharingPolicy.CanStartDownload(link, document, now));
        link.DownloadsStarted = 0; link.RevokedAt = now; Assert.False(SharingPolicy.IsActive(link, document, now));
        Assert.False(SharingPolicy.ValidLifetime(now, now.AddDays(8)));
    }

    private static Document Doc() => new() { WorkspaceId = WorkspaceId, UploaderId = Guid.NewGuid(), Title = "Synthetic invoice" };
    private static DocumentGrant Grant(Document document) => new() { WorkspaceId = WorkspaceId, DocumentId = document.Id, UserId = UserId, CanEdit = true, CanShare = true, CanExport = true };
}
