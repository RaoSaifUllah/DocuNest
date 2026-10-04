using DocuNest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace DocuNest.Infrastructure.Persistence;

public sealed partial class DocuNestDbContext
{
    private static void ConfigureIdentity(ModelBuilder model)
    {
        var users = model.Entity<UserAccount>();
        users.ToTable("users");
        users.HasKey(x => x.Id);
        users.HasIndex(x => x.NormalizedEmail).IsUnique();
        users.Property(x => x.Email).HasMaxLength(320);
        users.Property(x => x.NormalizedEmail).HasMaxLength(320);
        users.Property(x => x.DisplayName).HasMaxLength(160);
        users.Property(x => x.TimeZoneId).HasMaxLength(100);

        var sessions = model.Entity<RefreshSession>();
        sessions.ToTable("refresh_sessions");
        sessions.HasKey(x => x.Id);
        sessions.HasIndex(x => x.TokenHash).IsUnique();
        sessions.HasIndex(x => new { x.UserId, x.FamilyId });
        sessions.HasIndex(x => x.AbsoluteExpiresAt);
        sessions.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        sessions.HasOne<RefreshSession>().WithMany().HasForeignKey(x => x.ReplacedBySessionId).OnDelete(DeleteBehavior.Restrict);

        var tokens = model.Entity<CredentialToken>();
        tokens.ToTable("credential_tokens");
        tokens.HasKey(x => x.Id);
        tokens.HasIndex(x => x.TokenHash).IsUnique();
        tokens.HasIndex(x => x.ExpiresAt);
        tokens.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

        var recovery = model.Entity<MfaRecoveryCode>();
        recovery.ToTable("mfa_recovery_codes");
        recovery.HasKey(x => x.Id);
        recovery.HasIndex(x => new { x.UserId, x.CodeHash }).IsUnique();
        recovery.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

        var secrets = model.Entity<ConfigurationSecret>();
        secrets.ToTable("configuration_secrets");
        secrets.HasKey(x => x.Id);
        secrets.HasIndex(x => x.Name).IsUnique();
        secrets.Property(x => x.Ciphertext).HasColumnType("text");
    }

    private static void ConfigureWorkspaces(ModelBuilder model)
    {
        var workspaces = model.Entity<Workspace>();
        workspaces.ToTable("workspaces", table =>
        {
            table.HasCheckConstraint("ck_workspace_quota", "storage_quota_bytes > 0 AND used_storage_bytes >= 0 AND reserved_storage_bytes >= 0 AND used_storage_bytes + reserved_storage_bytes <= storage_quota_bytes");
            table.HasCheckConstraint("ck_workspace_retention", "retention_days BETWEEN 1 AND 3650");
        });
        workspaces.HasKey(x => x.Id);
        workspaces.Property(x => x.Name).HasMaxLength(160);

        Tenant<Membership>(model, "memberships");
        var members = model.Entity<Membership>();
        members.HasAlternateKey(x => new { x.WorkspaceId, x.UserId });
        members.HasIndex(x => new { x.UserId, x.WorkspaceId });
        members.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        members.ToTable("memberships", table => table.HasCheckConstraint("ck_membership_role", "role BETWEEN 0 AND 2"));

        Tenant<Invitation>(model, "invitations");
        var invitations = model.Entity<Invitation>();
        invitations.HasIndex(x => x.TokenHash).IsUnique();
        invitations.HasIndex(x => new { x.WorkspaceId, x.NormalizedEmail });
        invitations.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.InviterId).OnDelete(DeleteBehavior.Restrict);
        invitations.Property(x => x.NormalizedEmail).HasMaxLength(320);

        DocumentChild<UploadReservation>(model, "upload_reservations");
        var reservations = model.Entity<UploadReservation>();
        reservations.HasIndex(x => new { x.Status, x.ExpiresAt });
        reservations.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        reservations.ToTable("upload_reservations", table => table.HasCheckConstraint("ck_reservation_bytes", "reserved_bytes >= 0 AND received_bytes >= 0 AND received_bytes <= reserved_bytes"));
    }

    private static void ConfigureDocuments(ModelBuilder model)
    {
        Tenant<Category>(model, "categories");
        model.Entity<Category>().HasIndex(x => new { x.WorkspaceId, x.NormalizedName }).IsUnique();
        model.Entity<Category>().Property(x => x.Name).HasMaxLength(120);
        model.Entity<Category>().Property(x => x.NormalizedName).HasMaxLength(120);
        Tenant<Tag>(model, "tags");
        model.Entity<Tag>().HasIndex(x => new { x.WorkspaceId, x.NormalizedName }).IsUnique();
        model.Entity<Tag>().Property(x => x.Name).HasMaxLength(120);
        model.Entity<Tag>().Property(x => x.NormalizedName).HasMaxLength(120);

        Tenant<Document>(model, "documents");
        var documents = model.Entity<Document>();
        documents.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.UploaderId).OnDelete(DeleteBehavior.Restrict);
        documents.HasOne<Category>().WithMany().HasForeignKey(x => new { x.WorkspaceId, x.CategoryId })
            .HasPrincipalKey(x => new { x.WorkspaceId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        documents.HasIndex(x => new { x.WorkspaceId, x.TrashedAt, x.CreatedAt, x.Id });
        documents.HasIndex(x => new { x.WorkspaceId, x.ExpirationDate });
        documents.Property(x => x.Title).HasMaxLength(300);
        documents.Property(x => x.Description).HasColumnType("text");
        documents.Property(x => x.Vendor).HasMaxLength(300);
        documents.Property(x => x.Amount).HasPrecision(18, 2);
        documents.Property(x => x.Currency).HasMaxLength(3);
        documents.Property(x => x.CustomMetadataJson).HasColumnType("jsonb");
        documents.Property(x => x.ConfirmedFieldsJson).HasColumnType("jsonb");
        documents.ToTable("documents", table =>
        {
            table.HasCheckConstraint("ck_document_generation", "processing_generation > 0 AND page_count >= 0");
            table.HasCheckConstraint("ck_document_visibility", "visibility BETWEEN 0 AND 1");
            table.HasCheckConstraint("ck_document_currency", "currency IS NULL OR currency ~ '^[A-Z]{3}$'");
        });

        DocumentChild<DocumentGrant>(model, "document_grants");
        model.Entity<DocumentGrant>().HasIndex(x => new { x.WorkspaceId, x.DocumentId, x.UserId }).IsUnique();
        model.Entity<DocumentGrant>().HasOne<Membership>().WithMany()
            .HasForeignKey(x => new { x.WorkspaceId, x.UserId }).HasPrincipalKey(x => new { x.WorkspaceId, x.UserId })
            .OnDelete(DeleteBehavior.Cascade);

        DocumentChild<Blob>(model, "blobs");
        var blobs = model.Entity<Blob>();
        blobs.HasIndex(x => new { x.WorkspaceId, x.Sha256, x.Kind });
        blobs.HasIndex(x => x.StorageKey).IsUnique();
        blobs.HasIndex(x => new { x.WorkspaceId, x.DocumentId, x.Kind, x.Generation, x.PageNumber });
        blobs.Property(x => x.Sha256).HasMaxLength(64);
        blobs.Property(x => x.ContentType).HasMaxLength(100);
        blobs.Property(x => x.OriginalName).HasMaxLength(255);
        blobs.ToTable("blobs", table => table.HasCheckConstraint("ck_blob_bounds", "size_bytes >= 0 AND generation > 0 AND (page_number IS NULL OR page_number > 0) AND sha256 ~ '^[a-f0-9]{64}$'"));

        DocumentChild<OcrPage>(model, "ocr_pages");
        var pages = model.Entity<OcrPage>();
        pages.HasIndex(x => new { x.WorkspaceId, x.DocumentId, x.Generation, x.PageNumber }).IsUnique();
        pages.Property(x => x.RawText).HasColumnType("text");
        pages.Property(x => x.NormalizedText).HasColumnType("text");
        pages.ToTable("ocr_pages", table => table.HasCheckConstraint("ck_ocr_page_bounds", "page_number > 0 AND generation > 0 AND (confidence IS NULL OR confidence BETWEEN 0 AND 100)"));

        DocumentChild<ExtractionSuggestion>(model, "extraction_suggestions");
        model.Entity<ExtractionSuggestion>().HasIndex(x => new { x.WorkspaceId, x.DocumentId, x.Generation });
        model.Entity<ExtractionSuggestion>().ToTable("extraction_suggestions", table => table.HasCheckConstraint("ck_suggestion_confidence", "confidence BETWEEN 0 AND 1 AND generation > 0"));
        DocumentChild<DocumentTag>(model, "document_tags");
        model.Entity<DocumentTag>().HasIndex(x => new { x.WorkspaceId, x.DocumentId, x.TagId }).IsUnique();
        model.Entity<DocumentTag>().HasOne<Tag>().WithMany().HasForeignKey(x => new { x.WorkspaceId, x.TagId })
            .HasPrincipalKey(x => new { x.WorkspaceId, x.Id }).OnDelete(DeleteBehavior.Cascade);

        DocumentChild<SearchProjection>(model, "search_projections");
        var search = model.Entity<SearchProjection>();
        search.HasIndex(x => new { x.WorkspaceId, x.DocumentId }).IsUnique();
        search.Property(x => x.SearchText).HasColumnType("text");
        search.Property<NpgsqlTsVector>("SearchVector")
            .HasComputedColumnSql("to_tsvector('simple'::regconfig, coalesce(search_text, ''))", stored: true);
        search.HasIndex("SearchVector").HasMethod("GIN");
    }

    private static void ConfigureOperations(ModelBuilder model)
    {
        DocumentChild<Reminder>(model, "reminders");
        var reminders = model.Entity<Reminder>();
        reminders.HasOne<Membership>().WithMany().HasForeignKey(x => new { x.WorkspaceId, x.UserId })
            .HasPrincipalKey(x => new { x.WorkspaceId, x.UserId }).OnDelete(DeleteBehavior.Cascade);
        reminders.HasIndex(x => new { x.Status, x.DueAt });
        DocumentChild<Notification>(model, "notifications");
        var notifications = model.Entity<Notification>();
        notifications.HasOne<Reminder>().WithMany().HasForeignKey(x => new { x.WorkspaceId, x.ReminderId })
            .HasPrincipalKey(x => new { x.WorkspaceId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        notifications.HasIndex(x => new { x.WorkspaceId, x.ReminderId, x.Generation, x.Channel }).IsUnique();
        notifications.HasIndex(x => new { x.UserId, x.ReadAt });
        notifications.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        DocumentChild<ShareLink>(model, "share_links");
        var shares = model.Entity<ShareLink>();
        shares.HasIndex(x => x.TokenHash).IsUnique();
        shares.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.CreatorId).OnDelete(DeleteBehavior.Restrict);
        shares.ToTable("share_links", table => table.HasCheckConstraint("ck_share_download_budget", "downloads_started >= 0 AND (maximum_downloads IS NULL OR (maximum_downloads > 0 AND downloads_started <= maximum_downloads)) AND expires_at > created_at"));
        DocumentChild<ShareSession>(model, "share_sessions");
        var shareSessions = model.Entity<ShareSession>();
        shareSessions.HasIndex(x => x.TokenHash).IsUnique();
        shareSessions.HasOne<ShareLink>().WithMany().HasForeignKey(x => new { x.WorkspaceId, x.ShareLinkId })
            .HasPrincipalKey(x => new { x.WorkspaceId, x.Id }).OnDelete(DeleteBehavior.Cascade);

        DocumentChild<DurableJob>(model, "durable_jobs");
        var jobs = model.Entity<DurableJob>();
        jobs.Property(x => x.PayloadJson).HasColumnType("jsonb");
        jobs.HasIndex(x => new { x.WorkspaceId, x.DedupeKey }).IsUnique();
        jobs.HasIndex(x => new { x.Status, x.DueAt, x.LeaseExpiresAt });
        jobs.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.RequesterId).OnDelete(DeleteBehavior.Restrict);
        jobs.ToTable("durable_jobs", table => table.HasCheckConstraint("ck_job_attempts", "attempt_count >= 0 AND maximum_attempts BETWEEN 1 AND 100 AND fence >= 0"));
        Tenant<JobAttempt>(model, "job_attempts");
        model.Entity<JobAttempt>().HasOne<DurableJob>().WithMany().HasForeignKey(x => new { x.WorkspaceId, x.JobId })
            .HasPrincipalKey(x => new { x.WorkspaceId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        model.Entity<JobAttempt>().HasIndex(x => new { x.WorkspaceId, x.JobId, x.Fence }).IsUnique();

        Tenant<ExportJob>(model, "export_jobs");
        var exports = model.Entity<ExportJob>();
        exports.Property(x => x.DocumentIdsJson).HasColumnType("jsonb");
        exports.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.RequesterId).OnDelete(DeleteBehavior.Restrict);
        Tenant<ImportJob>(model, "import_jobs");
        model.Entity<ImportJob>().HasOne<UserAccount>().WithMany().HasForeignKey(x => x.RequesterId).OnDelete(DeleteBehavior.Restrict);
        DocumentChild<ImportItem>(model, "import_items");
        model.Entity<ImportItem>().Property(x => x.MetadataJson).HasColumnType("jsonb");
        model.Entity<ImportItem>().HasOne<ImportJob>().WithMany().HasForeignKey(x => new { x.WorkspaceId, x.ImportJobId })
            .HasPrincipalKey(x => new { x.WorkspaceId, x.Id }).OnDelete(DeleteBehavior.Cascade);

        var audit = model.Entity<AuditEvent>();
        audit.ToTable("audit_events");
        audit.HasKey(x => x.Id);
        audit.HasIndex(x => new { x.WorkspaceId, x.OccurredAt });
        audit.HasOne<Workspace>().WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        audit.HasOne<UserAccount>().WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
    }
}
