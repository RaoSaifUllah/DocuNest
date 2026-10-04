using Dapper;
using DocuNest.Application.Abstractions;
using DocuNest.Domain.Entities;
using DocuNest.Domain.Enums;
using DocuNest.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;

namespace DocuNest.IntegrationTests;

public sealed class PersistenceTests(PostgreSqlFixture fixture) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task EmptyDatabaseMigratesDownAndUpWithReviewedViews()
    {
        await fixture.ResetAsync();
        await using var db = fixture.CreateDb();
        Assert.Single(await db.Database.GetAppliedMigrationsAsync());
        await db.GetService<IMigrator>().MigrateAsync("0");
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await SearchPathGuardInterceptor.SelectAndVerifyAsync(connection);
        Assert.Null(await connection.ExecuteScalarAsync<string?>("SELECT to_regclass('documents')::text"));
        await db.Database.MigrateAsync();
        Assert.Equal("read_documents", await connection.ExecuteScalarAsync<string>("SELECT to_regclass('read_documents')::text"));
    }

    [Fact]
    public async Task UnitOfWorkRollbackIncludesDocumentAndDurableJob()
    {
        await fixture.ResetAsync();
        await using (var db = fixture.CreateDb())
        {
            var work = new UnitOfWork(db);
            await using var transaction = await work.BeginTransactionAsync();
            var user = User("rollback");
            var workspace = Workspace("rollback");
            var document = Document(workspace, user, "rolled back");
            new UserAccountRepository(db).Add(user);
            new WorkspaceRepository(db).Add(workspace);
            new DocumentRepository(db).Add(document);
            db.DurableJobs.Add(new DurableJob { WorkspaceId = workspace.Id, DocumentId = document.Id,
                DedupeKey = "rollback", DueAt = DateTimeOffset.UtcNow, CreatedAt = DateTimeOffset.UtcNow, Generation = 1 });
            await work.SaveChangesAsync();
            await transaction.RollbackAsync();
        }
        await using var verify = fixture.CreateDb();
        Assert.Equal(0, await verify.Documents.CountAsync());
        Assert.Equal(0, await verify.DurableJobs.CountAsync());
        Assert.Equal(0, await verify.Workspaces.CountAsync());
    }

    [Fact]
    public async Task CompositeForeignKeysRejectCrossWorkspaceBlobCategoryAndTag()
    {
        await fixture.ResetAsync();
        var (owner, _, first, second, documents) = await SeedAsync();
        await using (var db = fixture.CreateDb())
        {
            db.Blobs.Add(new Blob { WorkspaceId = second.Id, DocumentId = documents[0].Id, StorageKey = Guid.NewGuid().ToString("N"),
                Sha256 = new string('a', 64), Generation = 1 });
            var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
        }
        var category = new Category { WorkspaceId = second.Id, Name = "foreign", NormalizedName = "FOREIGN" };
        var tag = new Tag { WorkspaceId = second.Id, Name = "foreign", NormalizedName = "FOREIGN" };
        await using (var db = fixture.CreateDb())
        {
            db.AddRange(category, tag);
            await db.SaveChangesAsync();
        }
        await using (var db = fixture.CreateDb())
        {
            db.Documents.Add(Document(first, owner, "invalid category", category.Id));
            var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
        }
        await using (var db = fixture.CreateDb())
        {
            db.DocumentTags.Add(new DocumentTag { WorkspaceId = first.Id, DocumentId = documents[0].Id, TagId = tag.Id });
            var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, Assert.IsType<PostgresException>(error.InnerException).SqlState);
        }
    }

    [Fact]
    public async Task DapperTransactionIsServerReadOnlyAndProjectionColumnsExcludeSecrets()
    {
        await fixture.ResetAsync();
        await using (var session = await ReadOnlyProjectionSession.OpenAsync(fixture.ConnectionString))
        {
            var exception = await Assert.ThrowsAsync<PostgresException>(() => session.Connection.ExecuteAsync(
                "INSERT INTO workspaces (id,name,storage_quota_bytes,used_storage_bytes,reserved_storage_bytes,retention_days,created_at,version) VALUES (@Id,'denied',100,0,0,30,now(),1)",
                new { Id = Guid.NewGuid() }, session.Transaction));
            Assert.Equal(PostgresErrorCodes.ReadOnlySqlTransaction, exception.SqlState);
        }
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await SearchPathGuardInterceptor.SelectAndVerifyAsync(connection);
        var columns = (await connection.QueryAsync<string>("SELECT column_name FROM information_schema.columns WHERE table_schema = @Schema AND table_name LIKE 'read_%'", new { fixture.Schema })).ToArray();
        Assert.NotEmpty(columns);
        foreach (var forbidden in new[] { "password_hash", "token_hash", "csrf_hash", "security_stamp", "encrypted_mfa_secret", "ciphertext", "storage_key", "pending_storage_key" })
            Assert.DoesNotContain(forbidden, columns);
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>("SELECT count(*)::int FROM information_schema.views WHERE table_schema = @Schema AND table_name IN ('read_users','read_refresh_sessions','read_share_links','read_configuration_secrets')", new { fixture.Schema }));
    }

    [Fact]
    public async Task SearchDetailsCountsSnippetsAndDashboardShareThePermissionBoundary()
    {
        await fixture.ResetAsync();
        var (owner, member, workspace, other, documents) = await SeedAsync();
        var queries = new DapperDocumentQueries(fixture.ConnectionString);
        var result = await queries.SearchAsync(new DocumentSearchQuery { WorkspaceId = workspace.Id, UserId = member.Id, Query = "needle" });
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.DoesNotContain(result.Items, item => item.Id == documents[2].Id || item.Snippet?.Contains("hidden-marker", StringComparison.Ordinal) == true);
        Assert.Null(await queries.GetAsync(workspace.Id, documents[2].Id, member.Id));
        Assert.Null(await queries.GetAsync(other.Id, documents[0].Id, owner.Id));
        Assert.Equal(3, (await queries.SearchAsync(new DocumentSearchQuery { WorkspaceId = workspace.Id, UserId = owner.Id })).TotalCount);
        var outsider = Guid.NewGuid();
        Assert.Empty((await queries.SearchAsync(new DocumentSearchQuery { WorkspaceId = workspace.Id, UserId = outsider })).Items);
        Assert.Null(await queries.DashboardAsync(workspace.Id, outsider));
        Assert.Equal(2, (await queries.DashboardAsync(workspace.Id, member.Id))!.DocumentCount);
        var injection = await queries.SearchAsync(new DocumentSearchQuery { WorkspaceId = workspace.Id, UserId = member.Id,
            Vendor = "'; DELETE FROM users; --" });
        Assert.Empty(injection.Items);
        await using var db = fixture.CreateDb();
        Assert.Equal(2, await db.Users.CountAsync());
    }

    [Fact]
    public async Task OptimisticVersionRejectsLostUpdates()
    {
        await fixture.ResetAsync();
        var (_, _, workspace, _, documents) = await SeedAsync();
        await using var first = fixture.CreateDb();
        await using var second = fixture.CreateDb();
        var firstDocument = await new DocumentRepository(first).GetAsync(workspace.Id, documents[0].Id);
        var secondDocument = await new DocumentRepository(second).GetAsync(workspace.Id, documents[0].Id);
        firstDocument!.Title = "confirmed edit";
        await first.SaveChangesAsync();
        Assert.Equal(2, firstDocument.Version);
        secondDocument!.Title = "stale edit";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        await using var verify = fixture.CreateDb();
        Assert.Equal("confirmed edit", (await verify.Documents.SingleAsync(x => x.Id == documents[0].Id)).Title);
    }

    [Fact]
    public async Task SearchCursorIsPermissionScopedAndSortingIsAllowlisted()
    {
        await fixture.ResetAsync();
        var (_, member, workspace, _, documents) = await SeedAsync();
        var queries = new DapperDocumentQueries(fixture.ConnectionString);
        var first = await queries.SearchAsync(new DocumentSearchQuery { WorkspaceId = workspace.Id, UserId = member.Id, PageSize = 1 });
        Assert.Single(first.Items);
        Assert.NotNull(first.NextCursor);
        var next = await queries.SearchAsync(new DocumentSearchQuery { WorkspaceId = workspace.Id, UserId = member.Id, PageSize = 1, Cursor = first.NextCursor });
        Assert.Single(next.Items);
        Assert.NotEqual(first.Items[0].Id, next.Items[0].Id);
        Assert.Null(next.NextCursor);
        Assert.Empty((await queries.SearchAsync(new DocumentSearchQuery { WorkspaceId = workspace.Id, UserId = member.Id, Cursor = documents[2].Id })).Items);
        await Assert.ThrowsAsync<ArgumentException>(() => queries.SearchAsync(new DocumentSearchQuery { WorkspaceId = workspace.Id, UserId = member.Id, Sort = "title; DROP TABLE users" }));
    }

    private async Task<(UserAccount Owner, UserAccount Member, Workspace First, Workspace Second, Document[] Documents)> SeedAsync()
    {
        var owner = User("owner");
        var member = User("member");
        var first = Workspace("first");
        var second = Workspace("second");
        var documents = new[] { Document(first, owner, "Public"), Document(first, owner, "Granted"), Document(first, owner, "Private"), Document(second, owner, "Foreign") };
        documents[1].Visibility = DocumentVisibility.Restricted;
        documents[2].Visibility = DocumentVisibility.Restricted;
        await using var db = fixture.CreateDb();
        db.AddRange(owner, member, first, second);
        db.Memberships.AddRange(new Membership { WorkspaceId = first.Id, UserId = owner.Id, Role = WorkspaceRole.Owner },
            new Membership { WorkspaceId = second.Id, UserId = owner.Id, Role = WorkspaceRole.Owner },
            new Membership { WorkspaceId = first.Id, UserId = member.Id, Role = WorkspaceRole.Member });
        db.Documents.AddRange(documents);
        db.DocumentGrants.Add(new DocumentGrant { WorkspaceId = first.Id, DocumentId = documents[1].Id, UserId = member.Id });
        db.SearchProjections.AddRange(documents.Select((document, index) => new SearchProjection { WorkspaceId = document.WorkspaceId,
            DocumentId = document.Id, Generation = 1, SearchText = index == 2 ? "needle hidden-marker" : "needle synthetic receipt", PublishedAt = DateTimeOffset.UtcNow }));
        await db.SaveChangesAsync();
        return (owner, member, first, second, documents);
    }

    private static UserAccount User(string name) => new() { Email = name + "@example.invalid", NormalizedEmail = (name + "@example.invalid").ToUpperInvariant(),
        DisplayName = name, CreatedAt = DateTimeOffset.UtcNow, PasswordHash = "synthetic-only" };
    private static Workspace Workspace(string name) => new() { Name = name, CreatedAt = DateTimeOffset.UtcNow };
    private static Document Document(Workspace workspace, UserAccount user, string title, Guid? categoryId = null) => new()
    { WorkspaceId = workspace.Id, UploaderId = user.Id, Title = title, CategoryId = categoryId, ProcessingStatus = DocumentProcessingStatus.Completed,
        CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, IssueDate = new DateOnly(2026, 1, 1) };
}
