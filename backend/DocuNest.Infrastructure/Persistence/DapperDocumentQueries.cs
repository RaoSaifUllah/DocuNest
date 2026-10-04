using Dapper;
using DocuNest.Application.Abstractions;
using DocuNest.Domain.Enums;

namespace DocuNest.Infrastructure.Persistence;

public sealed class DapperDocumentQueries(string connectionString) : IDocumentQueries
{
    // The same predicate protects returned rows, cursor selection, counts and snippets.
    private const string VisibilityPredicate = """
        d.workspace_id = @WorkspaceId
        AND d.purge_requested_at IS NULL
        AND (m.role = @OwnerRole OR d.visibility = @WorkspaceVisibility
             OR d.uploader_id = @UserId OR EXISTS (
                SELECT 1 FROM read_document_grants g WHERE g.workspace_id = d.workspace_id
                AND g.document_id = d.id AND g.user_id = @UserId))
        """;

    public async Task<PageResult<DocumentSummary>> SearchAsync(DocumentSearchQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(query.PageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(query.PageSize, 100);
        if (query.TagIds.Count > 100 || query.Query?.Length > 512 || query.Vendor?.Length > 300)
            throw new ArgumentException("Search filters exceed the supported bounds.", nameof(query));

        // Only these fixed clauses may be interpolated into SQL; user values are parameters.
        var (order, cursor) = query.Sort switch
        {
            "created_desc" => ("created_at DESC, id DESC", "(p.created_at, p.id) < (c.created_at, c.id)"),
            "created_asc" => ("created_at ASC, id ASC", "(p.created_at, p.id) > (c.created_at, c.id)"),
            "title_asc" => ("title ASC, id ASC", "(p.title, p.id) > (c.title, c.id)"),
            "updated_desc" => ("updated_at DESC, id DESC", "(p.updated_at, p.id) < (c.updated_at, c.id)"),
            _ => throw new ArgumentException("Unsupported search sort.", nameof(query))
        };
        var parameters = new
        {
            query.WorkspaceId, query.UserId, query.CategoryId,
            IssuedFrom = query.IssuedFrom?.ToDateTime(TimeOnly.MinValue),
            IssuedTo = query.IssuedTo?.ToDateTime(TimeOnly.MinValue),
            query.IncludeTrash, query.Cursor,
            TagIds = query.TagIds.ToArray(),
            Query = string.IsNullOrWhiteSpace(query.Query) ? null : query.Query.Trim(),
            Vendor = string.IsNullOrWhiteSpace(query.Vendor) ? null : query.Vendor.Trim(),
            Status = query.Status is null ? (int?)null : (int)query.Status,
            Limit = query.PageSize + 1,
            OwnerRole = (int)WorkspaceRole.Owner,
            WorkspaceVisibility = (int)DocumentVisibility.Workspace
        };
        var permitted = $$"""
            WITH permitted AS (
                SELECT d.*, cat.name AS category_name,
                    CASE WHEN @Query::text IS NULL THEN NULL ELSE left(ts_headline('simple', sp.search_text,
                        websearch_to_tsquery('simple', @Query),
                        'StartSel=⟦, StopSel=⟧,MaxFragments=1,MaxWords=35,MinWords=10'), 500) END AS snippet
                FROM read_documents d
                JOIN read_memberships m ON m.workspace_id = d.workspace_id AND m.user_id = @UserId
                LEFT JOIN read_categories cat ON cat.workspace_id = d.workspace_id AND cat.id = d.category_id
                LEFT JOIN read_search_projections sp ON sp.workspace_id = d.workspace_id
                    AND sp.document_id = d.id AND sp.generation = d.processing_generation
                WHERE {{VisibilityPredicate}}
                AND (@IncludeTrash OR d.trashed_at IS NULL)
                AND (@CategoryId::uuid IS NULL OR d.category_id = @CategoryId)
                AND (@Status::integer IS NULL OR d.processing_status = @Status)
                AND (@IssuedFrom::date IS NULL OR d.issue_date >= @IssuedFrom::date)
                AND (@IssuedTo::date IS NULL OR d.issue_date <= @IssuedTo::date)
                AND (@Vendor::text IS NULL OR d.vendor ILIKE '%' || @Vendor || '%')
                AND (@Query::text IS NULL OR sp.search_vector @@ websearch_to_tsquery('simple', @Query))
                AND NOT EXISTS (SELECT 1 FROM unnest(@TagIds::uuid[]) requested(tag_id)
                    WHERE NOT EXISTS (SELECT 1 FROM read_document_tags dt
                        WHERE dt.workspace_id = d.workspace_id AND dt.document_id = d.id AND dt.tag_id = requested.tag_id))
            )
            """;
        var sql = permitted + " SELECT count(*) FROM permitted; " + permitted + $$"""
            SELECT p.id AS "Id", p.workspace_id AS "WorkspaceId", p.title AS "Title", p.vendor AS "Vendor",
                p.processing_status AS "ProcessingStatus", p.created_at AS "CreatedAt", p.updated_at AS "UpdatedAt",
                p.version AS "Version", p.page_count AS "PageCount", p.snippet AS "Snippet",
                p.category_name AS "CategoryName", p.expiration_date AS "ExpirationDate"
            FROM permitted p
            WHERE @Cursor::uuid IS NULL OR EXISTS (SELECT 1 FROM permitted c WHERE c.id = @Cursor AND {{cursor}})
            ORDER BY {{order}} LIMIT @Limit;
            """;
        await using var session = await ReadOnlyProjectionSession.OpenAsync(connectionString, cancellationToken);
        using var result = await session.Connection.QueryMultipleAsync(new CommandDefinition(sql, parameters,
            transaction: session.Transaction, cancellationToken: cancellationToken));
        var total = await result.ReadSingleAsync<long>();
        var rows = (await result.ReadAsync<SummaryRow>()).ToList();
        var hasMore = rows.Count > query.PageSize;
        if (hasMore) rows.RemoveAt(rows.Count - 1);
        return new PageResult<DocumentSummary>(rows.Select(row => row.ToSummary()).ToArray(), total,
            hasMore && rows.Count > 0 ? rows[^1].Id : null);
    }

    public async Task<DocumentDetails?> GetAsync(Guid workspaceId, Guid documentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var sql = $$"""
            SELECT d.id AS "Id", d.workspace_id AS "WorkspaceId", d.title AS "Title", d.description AS "Description",
                d.vendor AS "Vendor", d.amount AS "Amount", d.currency AS "Currency", d.issue_date AS "IssueDate",
                d.document_date AS "DocumentDate", d.expiration_date AS "ExpirationDate", d.category_id AS "CategoryId",
                d.custom_metadata_json::text AS "CustomMetadataJson", d.visibility AS "Visibility",
                d.processing_status AS "ProcessingStatus", d.page_count AS "PageCount",
                d.processing_generation AS "ProcessingGeneration", d.failure_code AS "FailureCode",
                d.trashed_at AS "TrashedAt", d.created_at AS "CreatedAt", d.updated_at AS "UpdatedAt", d.version AS "Version"
            FROM read_documents d JOIN read_memberships m ON m.workspace_id = d.workspace_id AND m.user_id = @UserId
            WHERE {{VisibilityPredicate}} AND d.id = @DocumentId
            """;
        await using var session = await ReadOnlyProjectionSession.OpenAsync(connectionString, cancellationToken);
        var row = await session.Connection.QuerySingleOrDefaultAsync<DetailsRow>(new CommandDefinition(sql,
            Scope(workspaceId, userId, documentId), transaction: session.Transaction, cancellationToken: cancellationToken));
        return row?.ToDetails();
    }

    public async Task<DashboardSummary?> DashboardAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        var sql = $$"""
            WITH permitted AS (
                SELECT d.* FROM read_documents d
                JOIN read_memberships m ON m.workspace_id = d.workspace_id AND m.user_id = @UserId
                WHERE {{VisibilityPredicate}}
            )
            SELECT (SELECT count(*) FROM permitted WHERE trashed_at IS NULL) AS "DocumentCount",
                (SELECT count(*) FROM permitted WHERE trashed_at IS NULL AND processing_status IN (0,1,2,3)) AS "ProcessingCount",
                (SELECT count(*) FROM permitted WHERE trashed_at IS NOT NULL) AS "TrashCount",
                (SELECT count(*) FROM permitted WHERE trashed_at IS NULL AND expiration_date BETWEEN CURRENT_DATE AND CURRENT_DATE + 30) AS "ExpiringCount",
                CASE WHEN m.role = @OwnerRole THEN w.used_storage_bytes ELSE
                    (SELECT coalesce(sum(b.size_bytes), 0)::bigint FROM read_blobs b JOIN permitted d
                        ON d.workspace_id = b.workspace_id AND d.id = b.document_id WHERE b.deleted_at IS NULL) END AS "StorageUsedBytes",
                w.storage_quota_bytes AS "StorageQuotaBytes",
                CASE WHEN m.role = @OwnerRole THEN w.reserved_storage_bytes ELSE 0 END AS "StorageReservedBytes"
            FROM read_workspaces w JOIN read_memberships m ON m.workspace_id = w.id AND m.user_id = @UserId
            WHERE w.id = @WorkspaceId
            """;
        await using var session = await ReadOnlyProjectionSession.OpenAsync(connectionString, cancellationToken);
        return await session.Connection.QuerySingleOrDefaultAsync<DashboardSummary>(new CommandDefinition(sql,
            Scope(workspaceId, userId), transaction: session.Transaction, cancellationToken: cancellationToken));
    }

    private static object Scope(Guid workspaceId, Guid userId, Guid? documentId = null) => new
    {
        WorkspaceId = workspaceId, UserId = userId, DocumentId = documentId,
        OwnerRole = (int)WorkspaceRole.Owner, WorkspaceVisibility = (int)DocumentVisibility.Workspace
    };

    private sealed class SummaryRow
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public string Title { get; set; } = "";
        public string? Vendor { get; set; }
        public int ProcessingStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long Version { get; set; }
        public int PageCount { get; set; }
        public string? Snippet { get; set; }
        public string? CategoryName { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DocumentSummary ToSummary() => new(Id, WorkspaceId, Title, Vendor,
            ((DocumentProcessingStatus)ProcessingStatus).ToString(), Utc(CreatedAt), Utc(UpdatedAt), Version,
            PageCount, Snippet, CategoryName, Date(ExpirationDate));
    }

    private sealed class DetailsRow
    {
        public Guid Id { get; set; }
        public Guid WorkspaceId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? Vendor { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? DocumentDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public Guid? CategoryId { get; set; }
        public string CustomMetadataJson { get; set; } = "{}";
        public int Visibility { get; set; }
        public int ProcessingStatus { get; set; }
        public int PageCount { get; set; }
        public long ProcessingGeneration { get; set; }
        public string? FailureCode { get; set; }
        public DateTime? TrashedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long Version { get; set; }
        public DocumentDetails ToDetails() => new(Id, WorkspaceId, Title, Description, Vendor, Amount, Currency,
            Date(IssueDate), Date(DocumentDate), Date(ExpirationDate), CategoryId, CustomMetadataJson,
            ((DocumentVisibility)Visibility).ToString(), ((DocumentProcessingStatus)ProcessingStatus).ToString(), PageCount,
            ProcessingGeneration, FailureCode, TrashedAt is null ? null : Utc(TrashedAt.Value), Utc(CreatedAt), Utc(UpdatedAt), Version);
    }

    private static DateTimeOffset Utc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    private static DateOnly? Date(DateTime? value) => value is null ? null : DateOnly.FromDateTime(value.Value);
}
