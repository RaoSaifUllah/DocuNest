using DocuNest.Domain.Enums;

namespace DocuNest.Domain.Entities;

public sealed class DurableJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? RequesterId { get; set; }
    public ProcessingStep Step { get; set; }
    public long Generation { get; set; }
    public JobStatus Status { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public string DedupeKey { get; set; } = "";
    public DateTimeOffset DueAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? LeaseOwner { get; set; }
    public DateTimeOffset? LeaseExpiresAt { get; set; }
    public long Fence { get; set; }
    public int AttemptCount { get; set; }
    public int MaximumAttempts { get; set; } = 5;
    public string? LastErrorCode { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class JobAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid JobId { get; set; }
    public long Fence { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string? ErrorCode { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class ExportJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid RequesterId { get; set; }
    public ExportScope Scope { get; set; }
    public string DocumentIdsJson { get; set; } = "[]";
    public bool IncludeTrash { get; set; }
    public int ManifestVersion { get; set; } = 1;
    public JobStatus Status { get; set; }
    public string? ArtifactStorageKey { get; set; }
    public string? ArtifactSha256 { get; set; }
    public long ArtifactBytes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string? FailureCode { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class ImportJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid RequesterId { get; set; }
    public int ManifestVersion { get; set; } = 1;
    public JobStatus Status { get; set; }
    public int TotalFiles { get; set; }
    public long TotalBytes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? FailureCode { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class ImportItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid ImportJobId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? SourceDocumentId { get; set; }
    public string RelativePath { get; set; } = "";
    public string MetadataJson { get; set; } = "{}";
    public long SizeBytes { get; set; }
    public ImportItemStatus Status { get; set; }
    public string? FailureCode { get; set; }
    public long Version { get; set; } = 1;
}

public sealed class AuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? WorkspaceId { get; set; }
    public Guid? ActorId { get; set; }
    public string Action { get; set; } = "";
    public string ResourceType { get; set; } = "";
    public Guid? ResourceId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string CorrelationId { get; set; } = "";
    public string Outcome { get; set; } = "success";
    public long Version { get; set; } = 1;
}
