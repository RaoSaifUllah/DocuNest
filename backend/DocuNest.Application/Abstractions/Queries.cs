using DocuNest.Domain.Enums;

namespace DocuNest.Application.Abstractions;

public sealed record DocumentSearchQuery
{
    public required Guid WorkspaceId { get; init; }
    public required Guid UserId { get; init; }
    public string? Query { get; init; }
    public Guid? CategoryId { get; init; }
    public IReadOnlyList<Guid> TagIds { get; init; } = [];
    public DocumentProcessingStatus? Status { get; init; }
    public DateOnly? IssuedFrom { get; init; }
    public DateOnly? IssuedTo { get; init; }
    public string? Vendor { get; init; }
    public bool IncludeTrash { get; init; }
    public string Sort { get; init; } = "created_desc";
    public Guid? Cursor { get; init; }
    public int PageSize { get; init; } = 25;
}

public sealed record DocumentSummary(Guid Id, Guid WorkspaceId, string Title, string? Vendor,
    string ProcessingStatus, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, long Version,
    int PageCount, string? Snippet, string? CategoryName, DateOnly? ExpirationDate);

public sealed record DocumentDetails(Guid Id, Guid WorkspaceId, string Title, string Description,
    string? Vendor, decimal? Amount, string? Currency, DateOnly? IssueDate, DateOnly? DocumentDate,
    DateOnly? ExpirationDate, Guid? CategoryId, string CustomMetadataJson, string Visibility,
    string ProcessingStatus, int PageCount, long ProcessingGeneration, string? FailureCode,
    DateTimeOffset? TrashedAt, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, long Version);

public sealed record PageResult<T>(IReadOnlyList<T> Items, long TotalCount, Guid? NextCursor);
public sealed record DashboardSummary(long DocumentCount, long ProcessingCount, long TrashCount,
    long ExpiringCount, long StorageUsedBytes, long StorageQuotaBytes, long StorageReservedBytes);

/// <summary>Every projection includes membership/visibility predicates, including counts and snippets.</summary>
public interface IDocumentQueries
{
    Task<PageResult<DocumentSummary>> SearchAsync(DocumentSearchQuery query, CancellationToken cancellationToken = default);
    Task<DocumentDetails?> GetAsync(Guid workspaceId, Guid documentId, Guid userId, CancellationToken cancellationToken = default);
    Task<DashboardSummary?> DashboardAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
}
