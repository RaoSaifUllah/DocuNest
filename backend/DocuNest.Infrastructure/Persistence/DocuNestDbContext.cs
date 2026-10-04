using System.Text.RegularExpressions;
using DocuNest.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuNest.Infrastructure.Persistence;

/// <summary>The command-side unit of work. Read projections use explicitly read-only transactions.</summary>
public sealed partial class DocuNestDbContext(DbContextOptions<DocuNestDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();
    public DbSet<CredentialToken> CredentialTokens => Set<CredentialToken>();
    public DbSet<MfaRecoveryCode> MfaRecoveryCodes => Set<MfaRecoveryCode>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentGrant> DocumentGrants => Set<DocumentGrant>();
    public DbSet<Blob> Blobs => Set<Blob>();
    public DbSet<OcrPage> OcrPages => Set<OcrPage>();
    public DbSet<ExtractionSuggestion> ExtractionSuggestions => Set<ExtractionSuggestion>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<DocumentTag> DocumentTags => Set<DocumentTag>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ShareLink> ShareLinks => Set<ShareLink>();
    public DbSet<ShareSession> ShareSessions => Set<ShareSession>();
    public DbSet<UploadReservation> UploadReservations => Set<UploadReservation>();
    public DbSet<ExportJob> ExportJobs => Set<ExportJob>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<ImportItem> ImportItems => Set<ImportItem>();
    public DbSet<DurableJob> DurableJobs => Set<DurableJob>();
    public DbSet<JobAttempt> JobAttempts => Set<JobAttempt>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<SearchProjection> SearchProjections => Set<SearchProjection>();
    public DbSet<ConfigurationSecret> ConfigurationSecrets => Set<ConfigurationSecret>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        ConfigureIdentity(model);
        ConfigureWorkspaces(model);
        ConfigureDocuments(model);
        ConfigureOperations(model);

        // Explicit, local convention: no provider naming extension or schema magic.
        foreach (var entity in model.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
                if (property.Name == "Version")
                {
                    property.IsConcurrencyToken = true;
                }
                if (property.ClrType == typeof(string) && property.GetMaxLength() is null && property.GetColumnType() is not ("text" or "jsonb"))
                {
                    property.SetMaxLength(2048);
                }
            }
            foreach (var key in entity.GetKeys())
            {
                key.SetName($"{(key.IsPrimaryKey() ? "pk" : "ak")}_{entity.GetTableName()}_{string.Join('_', key.Properties.Select(p => p.GetColumnName()))}");
            }
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName($"fk_{entity.GetTableName()}_{foreignKey.PrincipalEntityType.GetTableName()}_{string.Join('_', foreignKey.Properties.Select(p => p.GetColumnName()))}");
            }
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName($"ix_{entity.GetTableName()}_{string.Join('_', index.Properties.Select(p => p.GetColumnName()))}");
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AdvanceVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AdvanceVersions();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AdvanceVersions()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            var property = entry.Metadata.FindProperty("Version");
            if (property is null) continue;
            var version = entry.Property("Version");
            version.CurrentValue = entry.State == EntityState.Added ? 1L : checked((long)version.OriginalValue! + 1);
        }
    }

    private static void Tenant<TEntity>(ModelBuilder model, string table) where TEntity : class
    {
        var entity = model.Entity<TEntity>();
        entity.ToTable(table);
        entity.HasKey("Id");
        entity.HasAlternateKey("WorkspaceId", "Id");
        entity.HasIndex("WorkspaceId");
        entity.HasOne<Workspace>().WithMany().HasForeignKey("WorkspaceId").OnDelete(DeleteBehavior.Restrict);
    }

    private static void DocumentChild<TEntity>(ModelBuilder model, string table) where TEntity : class
    {
        Tenant<TEntity>(model, table);
        model.Entity<TEntity>().HasOne<Document>().WithMany()
            .HasForeignKey("WorkspaceId", "DocumentId").HasPrincipalKey("WorkspaceId", "Id")
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static string ToSnakeCase(string value) => BoundaryRegex().Replace(value, "$1_$2").ToLowerInvariant();

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex BoundaryRegex();
}
