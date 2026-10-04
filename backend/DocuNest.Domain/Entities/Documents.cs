using DocuNest.Domain.Enums;

namespace DocuNest.Domain.Entities;

public sealed class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid UploaderId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Vendor { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? DocumentDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public Guid? CategoryId { get; set; }
    public string CustomMetadataJson { get; set; } = "{}";
    public string ConfirmedFieldsJson { get; set; } = "[]";
    public string LogicalFolder { get; set; } = "";
    public DocumentVisibility Visibility { get; set; }
    public DocumentProcessingStatus ProcessingStatus { get; set; } = DocumentProcessingStatus.Quarantined;
    public long ProcessingGeneration { get; set; } = 1;
    public int PageCount { get; set; }
    public string? FailureCode { get; set; }
    public DateTimeOffset? TrashedAt { get; set; }
    public DateTimeOffset? PurgeRequestedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class DocumentGrant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public bool CanEdit { get; set; }
    public bool CanShare { get; set; }
    public bool CanExport { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class Blob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public string StorageKey { get; set; } = "";
    public string? PendingStorageKey { get; set; }
    public string OriginalName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = "";
    public BlobKind Kind { get; set; }
    public ScanStatus ScanStatus { get; set; }
    public long Generation { get; set; }
    public int? PageNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PromotedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class OcrPage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public int PageNumber { get; set; }
    public long Generation { get; set; }
    public string RawText { get; set; } = "";
    public string NormalizedText { get; set; } = "";
    public string Language { get; set; } = "eng";
    public double? Confidence { get; set; }
    public string Source { get; set; } = "ocr";
    public long Version { get; set; } = 1;
}

public sealed class ExtractionSuggestion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public string Field { get; set; } = "";
    public string Value { get; set; } = "";
    public double Confidence { get; set; }
    public string Source { get; set; } = "heuristic";
    public long Generation { get; set; }
    public bool Accepted { get; set; }
    public bool Dismissed { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class SearchProjection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public long Generation { get; set; }
    public string SearchText { get; set; } = "";
    public string LanguageConfig { get; set; } = "simple";
    public DateTimeOffset PublishedAt { get; set; }
    public long Version { get; set; } = 1;
}
